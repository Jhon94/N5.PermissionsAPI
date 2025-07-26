using Microsoft.OpenApi.Models;
using N5.Permissions.Application.Extensions;
using N5.Permissions.Infrastructure.Extensions;
using N5.Permissions.Infrastructure.Data.Context;
using N5.Permissions.API.Middleware;
using Serilog;
using Serilog.Events;
using N5.Permissions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Connection String: {connectionString}");
    Log.Information("Connection String: {ConnectionString}", connectionString);

    // Add layer dependencies
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    Console.WriteLine("All services configured via AddInfrastructure");

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

    // Middleware
    app.UseMiddleware<LoggingMiddleware>();
    app.UseMiddleware<ExceptionMiddleware>();

    // Swagger configuration
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

    // Pipeline configuration
    app.UseCors("AllowAll");
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    // Map controllers and health checks
    app.MapControllers();
    app.MapHealthChecks("/health");

    // Add a simple redirect from root to swagger
    app.MapGet("/", () => Results.Redirect("/swagger"));

    // Initialize services
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

        var isInMemory = context.Database.IsInMemory();
        var databaseType = isInMemory ? "InMemory" : "SQL Server";
        logger.LogInformation("Database type detected: {DatabaseType}", databaseType);

        if (!isInMemory)
        {
            var connectionString = context.Database.GetConnectionString();
            logger.LogInformation("Using connection: {ConnectionString}", connectionString);
        }
        else
        {
            logger.LogInformation("Using InMemory database");
        }

        logger.LogInformation("Ensuring database exists...");
        try
        {
            var dbCreated = await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database created (was missing): {DbCreated}", dbCreated);
            logger.LogInformation("Database connection successful!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot connect to database!");
            throw new InvalidOperationException("Cannot connect to database", ex);
        }

        if (!isInMemory)
        {
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
        }
        else
        {
            logger.LogInformation("InMemory database - tables created automatically by EF");
        }

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

        logger.LogInformation("{DatabaseType} database initialized successfully", databaseType);
        logger.LogInformation("=== DATABASE DIAGNOSTICS COMPLETED ===");

        // Test Elasticsearch
        try
        {
            var elasticsearchService = scope.ServiceProvider.GetService<N5.Permissions.Domain.Interfaces.IElasticsearchService>();
            if (elasticsearchService != null)
            {
                logger.LogInformation("Elasticsearch service available");
            }
            else
            {
                logger.LogInformation("Elasticsearch service not registered");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Elasticsearch not available - continuing without it");
        }

        // Test Kafka
        try
        {
            var kafkaService = scope.ServiceProvider.GetService<N5.Permissions.Domain.Interfaces.IKafkaProducerService>();
            if (kafkaService != null)
            {
                await kafkaService.SendMessageAsync(Guid.NewGuid(), "startup-test");
                logger.LogInformation("Kafka connected successfully");
            }
            else
            {
                logger.LogInformation("Kafka service not registered");
            }
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