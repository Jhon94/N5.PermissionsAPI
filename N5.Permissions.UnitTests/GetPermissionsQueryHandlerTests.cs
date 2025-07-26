using FluentAssertions;
using Moq;
using N5.Permissions.Application.Queries.GetPermissions;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.UnitTests
{
    public class GetPermissionsQueryHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IKafkaProducerService> _kafkaServiceMock;
        private readonly GetPermissionsQueryHandler _handler;

        public GetPermissionsQueryHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _kafkaServiceMock = new Mock<IKafkaProducerService>();

            _handler = new GetPermissionsQueryHandler(
                _unitOfWorkMock.Object,
                _kafkaServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllPermissions_WhenNoFiltersProvided()
        {
            // Arrange
            var query = new GetPermissionsQuery();

            var permissions = new List<Permission>
            {
                new Permission
                {
                    Id = 1,
                    EmployeeForename = "John",
                    EmployeeSurname = "Doe",
                    PermissionTypeId = 1,
                    PermissionDate = DateTime.Today,
                    PermissionType = new PermissionType { Id = 1, Description = "Vacation" }
                },
                new Permission
                {
                    Id = 2,
                    EmployeeForename = "Jane",
                    EmployeeSurname = "Smith",
                    PermissionTypeId = 2,
                    PermissionDate = DateTime.Today.AddDays(1),
                    PermissionType = new PermissionType { Id = 2, Description = "Sick Leave" }
                }
            };

            _unitOfWorkMock.Setup(x => x.Permissions.GetAllAsync())
                .ReturnsAsync(permissions);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.First().Id.Should().Be(1);
            result.First().EmployeeForename.Should().Be("John");
            result.Last().Id.Should().Be(2);
            result.Last().EmployeeForename.Should().Be("Jane");

            _kafkaServiceMock.Verify(x => x.SendMessageAsync(It.IsAny<Guid>(), "get"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldFilterByEmployeeName_WhenEmployeeNameProvided()
        {
            // Arrange
            var query = new GetPermissionsQuery { EmployeeName = "John" };

            var permissions = new List<Permission>
            {
                new Permission
                {
                    Id = 1,
                    EmployeeForename = "John",
                    EmployeeSurname = "Doe",
                    PermissionType = new PermissionType { Description = "Vacation" }
                },
                new Permission
                {
                    Id = 2,
                    EmployeeForename = "Jane",
                    EmployeeSurname = "Smith",
                    PermissionType = new PermissionType { Description = "Sick Leave" }
                }
            };

            _unitOfWorkMock.Setup(x => x.Permissions.GetAllAsync())
                .ReturnsAsync(permissions);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.First().EmployeeForename.Should().Be("John");
        }
    }
}
