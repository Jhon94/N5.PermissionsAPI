using Microsoft.OpenApi.Models;
using N5.Permissions.Application.Extensions;
using N5.Permissions.Infrastructure.Data.Context;
using N5.Permissions.API.Middleware;
using Serilog;
using Serilog.Events;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using N5.Permissions.Infrastructure.Services;
using Nest;
using N5.Permissions.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "N5.Permissions.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/n5-permissions-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
Log.Information("Starting N5 Permissions API");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with detailed settings
builder.Services.AddSwaggerGen(c =>
{
c.SwaggerDoc("v1", new OpenApiInfo
{
Title = "N5 Permissions API",
Version = "v1",
Description = "A comprehensive Web API for managing user permissions",
Contact = new OpenApiContact
{
Name = "N5 Support",
Email = "support@n5now.com"
}
});

// Include XML comments if available
var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
if (File.Exists(xmlPath))
{
c.IncludeXmlComments(xmlPath);
}
});

// DIAGNÓSTICO: Verificar connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection String: {connectionString}");
Log.Information("Connection String: {ConnectionString}", connectionString);

// Add layer dependencies
builder.Services.AddApplication();

// SQL SERVER CONFIGURATION CON DIAGNÓSTICOS
Console.WriteLine("Configuring DbContext...");
builder.Services.AddDbContext<PermissionsDbContext>(options =>
{
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    if (builder.Environment.IsDevelopment())
{
options.EnableSensitiveDataLogging();
options.EnableDetailedErrors();
options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
}
});
Console.WriteLine("DbContext configured successfully");

// Repositories
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPermissionTypeRepository, PermissionTypeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Elasticsearch configuration
builder.Services.AddSingleton<IElasticClient>(serviceProvider =>
{
var uri = builder.Configuration["ElasticsearchSettings:Uri"] ?? "http://localhost:9200";
var connectionSettings = new ConnectionSettings(new Uri(uri))
    .DefaultIndex("permissions")
    .DisableDirectStreaming()
    .ThrowExceptions(false)
    .ServerCertificateValidationCallback((sender, certificate, chain, errors) => true)
    .RequestTimeout(TimeSpan.FromSeconds(10));

return new ElasticClient(connectionSettings);
});

builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();
builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService>();

// Add CORS for development
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowAll", policy =>
{
policy.AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader();
});
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PermissionsDbContext>("Database");

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger(c =>
{
c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
c.SwaggerEndpoint("/swagger/v1/swagger.json", "N5 Permissions API V1");
c.RoutePrefix = "swagger";
c.DisplayRequestDuration();
c.EnableDeepLinking();
c.EnableFilter();
c.ShowExtensions();
c.DocumentTitle = "N5 Permissions API Documentation";
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

// Add a simple redirect from root to swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

await InitializeServicesAsync(app.Services);

// Log startup information
var urls = builder.Configuration["ASPNETCORE_URLS"] ?? "https://localhost:7051;http://localhost:5000";
Log.Information("N5 Permissions API started successfully");
Log.Information("Swagger UI available at: https://localhost:7051/swagger");
Log.Information("Health check available at: https://localhost:7051/health");

app.Run();
}
catch (Exception ex)
{
Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
Log.CloseAndFlush();
}

static async Task InitializeServicesAsync(IServiceProvider serviceProvider)
{
using var scope = serviceProvider.CreateScope();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
logger.LogInformation("=== STARTING DATABASE DIAGNOSTICS ===");
var context = scope.ServiceProvider.GetRequiredService<PermissionsDbContext>();

// PASO 1: Verificar connection string
var connectionString = context.Database.GetConnectionString();
logger.LogInformation("Using connection: {ConnectionString}", connectionString);

// PASO 2: Verificar conectividad
logger.LogInformation("Testing database connection...");
var canConnect = await context.Database.CanConnectAsync();
logger.LogInformation("Can connect to database: {CanConnect}", canConnect);

if (!canConnect)
{
logger.LogError("Cannot connect to database! Check SQL Server is running.");
throw new InvalidOperationException("Cannot connect to database");
}

// PASO 3: Intentar crear la base de datos
logger.LogInformation("🏗️ Ensuring database exists...");
var dbCreated = await context.Database.EnsureCreatedAsync();
logger.LogInformation("🏗️ Database created (was missing): {DbCreated}", dbCreated);

// PASO 4: Verificar tablas existentes
logger.LogInformation("Checking existing tables...");
try
{
var sql = "SELECT name FROM sys.tables WHERE type = 'U'";
var command = context.Database.GetDbConnection().CreateCommand();
command.CommandText = sql;

await context.Database.OpenConnectionAsync();
using var reader = await command.ExecuteReaderAsync();
var tables = new List<string>();
while (await reader.ReadAsync())
{
tables.Add(reader.GetString(0));
}
await context.Database.CloseConnectionAsync();

logger.LogInformation("Found {Count} tables: {Tables}", tables.Count, string.Join(", ", tables));

if (tables.Contains("PermissionTypes"))
{
logger.LogInformation("PermissionTypes table exists!");
}
else
{
logger.LogWarning("PermissionTypes table NOT found!");
}

if (tables.Contains("Permissions"))
{
logger.LogInformation("Permissions table exists!");
}
else
{
logger.LogWarning("Permissions table NOT found!");
}
}
catch (Exception ex)
{
logger.LogError(ex, "Error listing tables");
}

// PASO 5: Verificar datos en PermissionTypes
logger.LogInformation("Checking PermissionTypes data...");
try
{
var permissionTypesCount = await context.PermissionTypes.CountAsync();
logger.LogInformation("PermissionTypes count: {Count}", permissionTypesCount);

if (permissionTypesCount == 0)
{
logger.LogWarning("No PermissionTypes found. Adding seed data manually...");

var permissionTypes = new List<PermissionType>
                {
                    new PermissionType { Description = "Vacation Leave" },
                    new PermissionType { Description = "Sick Leave" },
                    new PermissionType { Description = "Personal Leave" },
                    new PermissionType { Description = "Maternity/Paternity Leave" },
                    new PermissionType { Description = "Emergency Leave" }
                };

context.PermissionTypes.AddRange(permissionTypes);
await context.SaveChangesAsync();

var finalCount = await context.PermissionTypes.CountAsync();
logger.LogInformation("Manual seed data added. {Count} permission types created.", finalCount);
}
else
{
logger.LogInformation("Found {Count} permission types in database", permissionTypesCount);

var types = await context.PermissionTypes.Take(3).ToListAsync();
foreach (var type in types)
{
logger.LogInformation("PermissionType: ID={Id}, Description='{Description}'", type.Id, type.Description);
}
}
}
catch (Exception ex)
{
logger.LogError(ex, "Error accessing PermissionTypes table");
throw;
}

logger.LogInformation("SQL Server database initialized successfully");
logger.LogInformation("=== DATABASE DIAGNOSTICS COMPLETED ===");

// Test Elasticsearch
try
{
var elasticClient = scope.ServiceProvider.GetRequiredService<IElasticClient>();
var healthResponse = await elasticClient.Cluster.HealthAsync();

if (healthResponse.IsValid)
{
logger.LogInformation("Elasticsearch connected successfully - Status: {Status}", healthResponse.Status);

// Crear índice si no existe
var indexExists = await elasticClient.Indices.ExistsAsync("permissions");
if (!indexExists.Exists)
{
var createIndexResponse = await elasticClient.Indices.CreateAsync("permissions", c => c
    .Map<Permission>(m => m.AutoMap()));

if (createIndexResponse.IsValid)
{
logger.LogInformation("Elasticsearch index 'permissions' created successfully");
}
}
}
else
{
logger.LogWarning("Elasticsearch connection failed - continuing without it");
}
}
catch (Exception ex)
{
logger.LogWarning(ex, "Elasticsearch not available - continuing without it");
}

// Test Kafka
try
{
var kafkaService = scope.ServiceProvider.GetRequiredService<IKafkaProducerService>();
await kafkaService.SendMessageAsync(Guid.NewGuid(), "startup-test");
logger.LogInformation("Kafka connected successfully");
}
catch (Exception ex)
{
logger.LogWarning(ex, "Kafka not available - continuing without it");
}

logger.LogInformation("ALL SERVICES INITIALIZED SUCCESSFULLY");
}
catch (Exception ex)
{
logger.LogError(ex, "CRITICAL ERROR during service initialization");
throw;
}
}

public partial class Program { }