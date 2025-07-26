using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Data.Context;
using N5.Permissions.Infrastructure.Data.Repositories;
using System.Net;

namespace N5.Permissions.UnitTests
{
    public class PermissionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public PermissionsControllerTests(WebApplicationFactory<Program> factory)
        {
            var customFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IPermissionRepository, PermissionRepository>();
                    services.AddScoped<IPermissionTypeRepository, PermissionTypeRepository>();
                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<PermissionsDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add InMemory database
                    services.AddDbContext<PermissionsDbContext>(options =>
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

                    // Initialize database
                    var serviceProvider = services.BuildServiceProvider();
                    using var scope = serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<PermissionsDbContext>();
                    context.Database.EnsureCreated();

                    // Seed test data
                    if (!context.PermissionTypes.Any())
                    {
                        context.PermissionTypes.AddRange(
                            new Domain.Entities.PermissionType { Id = 1, Description = "Vacation Leave" },
                            new Domain.Entities.PermissionType { Id = 2, Description = "Sick Leave" }
                        );
                        context.SaveChanges();
                    }
                });
            });

            _client = customFactory.CreateClient();
        }

        [Fact]
        public async Task Health_ShouldReturn_Ok()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetPermissionTypes_ShouldReturn_Ok()
        {
            // Act
            var response = await _client.GetAsync("/api/permissiontypes");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
