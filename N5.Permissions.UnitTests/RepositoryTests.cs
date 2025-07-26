using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Infrastructure.Data.Context;
using N5.Permissions.Infrastructure.Data.Repositories;

namespace N5.Permissions.UnitTests
{
    public class RepositoryTests : IDisposable
    {
        private readonly PermissionsDbContext _context;
        private readonly PermissionRepository _permissionRepository;
        private readonly PermissionTypeRepository _permissionTypeRepository;

        public RepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PermissionsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PermissionsDbContext(options);
            _context.Database.EnsureCreated();

            _permissionRepository = new PermissionRepository(_context);
            _permissionTypeRepository = new PermissionTypeRepository(_context);
        }

        [Fact]
        public async Task PermissionRepository_ShouldAddAndRetrievePermission_WhenValidDataProvided()
        {
            // Arrange
            var permissionType = new PermissionType { Description = "Test Permission Type" };
            await _permissionTypeRepository.AddAsync(permissionType);
            await _context.SaveChangesAsync();

            var permission = new Permission
            {
                EmployeeForename = "John",
                EmployeeSurname = "Doe",
                PermissionTypeId = permissionType.Id,
                PermissionDate = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var addedPermission = await _permissionRepository.AddAsync(permission);
            await _context.SaveChangesAsync();

            var retrievedPermission = await _permissionRepository.GetByIdAsync(addedPermission.Id);

            // Assert
            retrievedPermission.Should().NotBeNull();
            retrievedPermission!.EmployeeForename.Should().Be("John");
            retrievedPermission.EmployeeSurname.Should().Be("Doe");
            retrievedPermission.PermissionTypeId.Should().Be(permissionType.Id);
            retrievedPermission.PermissionType.Should().NotBeNull();
            retrievedPermission.PermissionType.Description.Should().Be("Test Permission Type");
        }

        [Fact]
        public async Task PermissionRepository_ShouldReturnAllPermissions_WhenMultiplePermissionsExist()
        {
            // Arrange
            var permissionType = new PermissionType { Description = "Test Type" };
            await _permissionTypeRepository.AddAsync(permissionType);
            await _context.SaveChangesAsync();

            var permission1 = new Permission
            {
                EmployeeForename = "John",
                EmployeeSurname = "Doe",
                PermissionTypeId = permissionType.Id,
                PermissionDate = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            var permission2 = new Permission
            {
                EmployeeForename = "Jane",
                EmployeeSurname = "Smith",
                PermissionTypeId = permissionType.Id,
                PermissionDate = DateTime.Today.AddDays(1),
                CreatedAt = DateTime.UtcNow
            };

            await _permissionRepository.AddAsync(permission1);
            await _permissionRepository.AddAsync(permission2);
            await _context.SaveChangesAsync();

            // Act
            var permissions = await _permissionRepository.GetAllAsync();

            // Assert
            permissions.Should().HaveCount(2);
            permissions.Should().Contain(p => p.EmployeeForename == "John");
            permissions.Should().Contain(p => p.EmployeeForename == "Jane");
        }

        [Fact]
        public async Task PermissionRepository_ShouldFilterByEmployee_WhenEmployeeNameProvided()
        {
            // Arrange
            var permissionType = new PermissionType { Description = "Test Type" };
            await _permissionTypeRepository.AddAsync(permissionType);
            await _context.SaveChangesAsync();

            var permission1 = new Permission
            {
                EmployeeForename = "John",
                EmployeeSurname = "Doe",
                PermissionTypeId = permissionType.Id,
                PermissionDate = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            var permission2 = new Permission
            {
                EmployeeForename = "Jane",
                EmployeeSurname = "Smith",
                PermissionTypeId = permissionType.Id,
                PermissionDate = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            await _permissionRepository.AddAsync(permission1);
            await _permissionRepository.AddAsync(permission2);
            await _context.SaveChangesAsync();

            // Act
            var permissions = await _permissionRepository.GetByEmployeeAsync("John", "Doe");

            // Assert
            permissions.Should().HaveCount(1);
            permissions.First().EmployeeForename.Should().Be("John");
            permissions.First().EmployeeSurname.Should().Be("Doe");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
