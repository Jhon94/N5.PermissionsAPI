using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace N5.Permissions.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Add MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Add AutoMapper (opcional si decido usarlo)
            // services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Add FluentValidation (opcional si decido usarlo)
            // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
