using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using N5.Permissions.Application.Commands.ModifyPermission;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Exceptions;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.UnitTests
{
    public class ModifyPermissionCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IElasticsearchService> _elasticsearchServiceMock;
        private readonly Mock<IKafkaProducerService> _kafkaServiceMock;
        private readonly ModifyPermissionCommandHandler _handler;

        public ModifyPermissionCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _elasticsearchServiceMock = new Mock<IElasticsearchService>();
            _kafkaServiceMock = new Mock<IKafkaProducerService>();

            _handler = new ModifyPermissionCommandHandler(
                _unitOfWorkMock.Object,
                _elasticsearchServiceMock.Object,
                _kafkaServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldModifyPermission_WhenValidCommandProvided()
        {
            // Arrange
            var command = new ModifyPermissionCommand
            {
                Id = 1,
                EmployeeForename = "Jane",
                EmployeeSurname = "Smith",
                PermissionTypeId = 2,
                PermissionDate = DateTime.Today.AddDays(1)
            };

            var existingPermission = new Permission
            {
                Id = 1,
                EmployeeForename = "John",
                EmployeeSurname = "Doe",
                PermissionTypeId = 1,
                PermissionDate = DateTime.Today,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var permissionType = new PermissionType { Id = 2, Description = "Sick Leave" };

            _unitOfWorkMock.Setup(x => x.Permissions.GetByIdAsync(1))
                .ReturnsAsync(existingPermission);
            _unitOfWorkMock.Setup(x => x.PermissionTypes.GetByIdAsync(2))
                .ReturnsAsync(permissionType);
            _unitOfWorkMock.Setup(x => x.Permissions.UpdateAsync(It.IsAny<Permission>()))
                .ReturnsAsync(existingPermission);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.EmployeeForename.Should().Be("Jane");
            result.EmployeeSurname.Should().Be("Smith");
            result.PermissionTypeId.Should().Be(2);

            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
            _elasticsearchServiceMock.Verify(x => x.UpdatePermissionAsync(It.IsAny<Permission>()), Times.Once);
            _kafkaServiceMock.Verify(x => x.SendMessageAsync(It.IsAny<Guid>(), "modify"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenPermissionNotFound()
        {
            // Arrange
            var command = new ModifyPermissionCommand
            {
                Id = 999,
                EmployeeForename = "Jane",
                EmployeeSurname = "Smith",
                PermissionTypeId = 2,
                PermissionDate = DateTime.Today
            };

            _unitOfWorkMock.Setup(x => x.Permissions.GetByIdAsync(999))
                .ReturnsAsync((Permission?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<PermissionNotFoundException>(
                () => _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Contain("999");
            _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }
}
