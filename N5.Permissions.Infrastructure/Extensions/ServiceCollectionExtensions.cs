using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Data.Context;
using N5.Permissions.Infrastructure.Data.Repositories;
using N5.Permissions.Infrastructure.Services;
using Nest;

namespace N5.Permissions.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<PermissionsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IPermissionTypeRepository, PermissionTypeRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Elasticsearch
            services.AddSingleton<IElasticClient>(serviceProvider =>
            {
                var uri = configuration["ElasticsearchSettings:Uri"] ?? "http://localhost:9200";
                var connectionSettings = new ConnectionSettings(new Uri(uri))
                    .DefaultIndex("permissions")
                    .DisableDirectStreaming();

                return new ElasticClient(connectionSettings);
            });

            services.AddScoped<IElasticsearchService, ElasticsearchService>();

            // Kafka
            services.AddScoped<IKafkaProducerService, KafkaProducerService>();

            return services;
        }
    }
}
