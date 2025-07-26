using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using N5.Permissions.Application.Commands.RequestPermission;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Exceptions;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.UnitTests
{
    public class RequestPermissionCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IElasticsearchService> _elasticsearchServiceMock;
        private readonly Mock<IKafkaProducerService> _kafkaServiceMock;
        private readonly Mock<ILogger<RequestPermissionCommandHandler>> _loggerMock;
        private readonly RequestPermissionCommandHandler _handler;

        public RequestPermissionCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _elasticsearchServiceMock = new Mock<IElasticsearchService>();
            _kafkaServiceMock = new Mock<IKafkaProducerService>();
            _loggerMock = new Mock<ILogger<RequestPermissionCommandHandler>>();

            _handler = new RequestPermissionCommandHandler(
                _unitOfWorkMock.Object,
                _elasticsearchServiceMock.Object,
                _kafkaServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreatePermission_WhenValidCommandProvided()
        {
            // Arrange
            var command = new RequestPermissionCommand
            {
                EmployeeForename = "John",
                EmployeeSurname = "Doe",
                PermissionTypeId = 1,
                PermissionDate = DateTime.Today
            };

            var permissionType = new PermissionType { Id = 1, Description = "Vacation" };
            var permission = new Permission
            {
                Id = 1,
                EmployeeForename = command.EmployeeForename,
                EmployeeSurname = command.EmployeeSurname,
                PermissionTypeId = command.PermissionTypeId,
                PermissionDate = command.PermissionDate,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWorkMock.Setup(x => x.PermissionTypes.GetByIdAsync(1))
                .ReturnsAsync(permissionType);
            _unitOfWorkMock.Setup(x => x.Permissions.AddAsync(It.IsAny<Permission>()))
                .ReturnsAsync(permission);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.EmployeeForename.Should().Be("John");
            result.EmployeeSurname.Should().Be("Doe");
            result.PermissionTypeId.Should().Be(1);

            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
            _elasticsearchServiceMock.Verify(x => x.IndexPermissionAsync(It.IsAny<Permission>()), Times.Once);
            _kafkaServiceMock.Verify(x => x.SendMessageAsync(It.IsAny<Guid>(), "request"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenPermissionTypeNotFound()
        {
            // Arrange
            var command = new RequestPermissionCommand
            {
                EmployeeForename = "John",
                EmployeeSurname = "Doe",
                PermissionTypeId = 999,
                PermissionDate = DateTime.Today
            };

            _unitOfWorkMock.Setup(x => x.PermissionTypes.GetByIdAsync(999))
                .ReturnsAsync((PermissionType?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<PermissionTypeNotFoundException>(
                () => _handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Contain("999");
            _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }
}
