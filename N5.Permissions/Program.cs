using Microsoft.OpenApi.Models;
using N5.Permissions.Application.Extensions;
using N5.Permissions.Infrastructure.Extensions;
using N5.Permissions.Infrastructure.Data.Context;
using N5.Permissions.API.Middleware;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Annotations;

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
    Log.Information("🚀 Starting N5 Permissions API");

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

    // Add layer dependencies
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    //builder.Services.AddDbContext<PermissionsDbContext>(options =>
    //options.UseInMemoryDatabase("N5PermissionsInMemory"));


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

    // IMPORTANTE: Orden del middleware
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
        // Inicializar Base de Datos
        logger.LogInformation("Initializing database...");
        //var context = scope.ServiceProvider.GetRequiredService<PermissionsDbContext>();
        //await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Database initialized successfully");

        // Inicializar Elasticsearch (si está configurado)
        try
        {
            var elasticsearchService = scope.ServiceProvider.GetService<N5.Permissions.Domain.Interfaces.IElasticsearchService>();
            if (elasticsearchService != null)
            {
                logger.LogInformation("Elasticsearch service initialized");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Elasticsearch not available (optional for development)");
        }

        // Inicializar Kafka (si está configurado)
        try
        {
            var kafkaService = scope.ServiceProvider.GetService<N5.Permissions.Domain.Interfaces.IKafkaProducerService>();
            if (kafkaService != null)
            {
                logger.LogInformation("Kafka service initialized");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Kafka not available (optional for development)");
        }

        logger.LogInformation("All services initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during service initialization");
        throw;
    }
}