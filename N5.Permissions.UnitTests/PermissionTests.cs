using FluentAssertions;
using N5.Permissions.Domain.Entities;

namespace N5.Permissions.UnitTests
{
    public class PermissionTests
    {
        [Fact]
        public void Permission_ShouldCreateValidPermission_WhenValidDataProvided()
        {
            // Arrange
            var permission = new Permission
            {
                Id = 1,
                EmployeeForename = "John",
                EmployeeSurname = "Doe",
                PermissionTypeId = 1,
                PermissionDate = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            // Act & Assert
            permission.Id.Should().Be(1);
            permission.EmployeeForename.Should().Be("John");
            permission.EmployeeSurname.Should().Be("Doe");
            permission.PermissionTypeId.Should().Be(1);
            permission.PermissionDate.Should().Be(DateTime.Today);
            permission.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void GetFullName_ShouldReturnCombinedName_WhenCalled()
        {
            // Arrange
            var permission = new Permission
            {
                EmployeeForename = "Jane",
                EmployeeSurname = "Smith"
            };

            // Act
            var fullName = permission.GetFullName();

            // Assert
            fullName.Should().Be("Jane Smith");
        }

        [Fact]
        public void UpdatePermission_ShouldUpdateAllFields_WhenCalled()
        {
            // Arrange
            var permission = new Permission
            {
                EmployeeForename = "Old",
                EmployeeSurname = "Name",
                PermissionTypeId = 1,
                PermissionDate = DateTime.Today,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var newDate = DateTime.Today.AddDays(1);

            // Act
            permission.UpdatePermission("New", "Name", 2, newDate);

            // Assert
            permission.EmployeeForename.Should().Be("New");
            permission.EmployeeSurname.Should().Be("Name");
            permission.PermissionTypeId.Should().Be(2);
            permission.PermissionDate.Should().Be(newDate);
            permission.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}