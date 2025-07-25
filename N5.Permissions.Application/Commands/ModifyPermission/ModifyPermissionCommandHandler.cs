using MediatR;
using Microsoft.Extensions.Logging;
using N5.Permissions.Application.DTOs;
using N5.Permissions.Domain.Exceptions;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Application.Commands.ModifyPermission
{
    public class ModifyPermissionCommandHandler : IRequestHandler<ModifyPermissionCommand, PermissionDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly IKafkaProducerService _kafkaService;
        private readonly ILogger<ModifyPermissionCommandHandler> _logger;

        public ModifyPermissionCommandHandler(
            IUnitOfWork unitOfWork,
            IElasticsearchService elasticsearchService,
            IKafkaProducerService kafkaService,
            ILogger<ModifyPermissionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _elasticsearchService = elasticsearchService;
            _kafkaService = kafkaService;
            _logger = logger;
        }

        public async Task<PermissionDto> Handle(ModifyPermissionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing modify permission for ID: {PermissionId}", request.Id);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Get existing permission
                var permission = await _unitOfWork.Permissions.GetByIdAsync(request.Id);
                if (permission == null)
                {
                    throw new PermissionNotFoundException(request.Id);
                }

                // Verify permission type exists
                var permissionType = await _unitOfWork.PermissionTypes.GetByIdAsync(request.PermissionTypeId);
                if (permissionType == null)
                {
                    throw new PermissionTypeNotFoundException(request.PermissionTypeId);
                }

                // Update permission
                permission.UpdatePermission(
                    request.EmployeeForename,
                    request.EmployeeSurname,
                    request.PermissionTypeId,
                    request.PermissionDate);

                // Save to database
                var updatedPermission = await _unitOfWork.Permissions.UpdateAsync(permission);
                await _unitOfWork.SaveChangesAsync();

                // Create DTO for response
                var permissionDto = new PermissionDto
                {
                    Id = updatedPermission.Id,
                    EmployeeForename = updatedPermission.EmployeeForename,
                    EmployeeSurname = updatedPermission.EmployeeSurname,
                    PermissionTypeId = updatedPermission.PermissionTypeId,
                    PermissionTypeDescription = permissionType.Description,
                    PermissionDate = updatedPermission.PermissionDate,
                    CreatedAt = updatedPermission.CreatedAt,
                    UpdatedAt = updatedPermission.UpdatedAt
                };

                // Update in Elasticsearch
                await _elasticsearchService.UpdatePermissionAsync(updatedPermission);

                // Send message to Kafka
                await _kafkaService.SendMessageAsync(Guid.NewGuid(), "modify");

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Permission modified successfully with ID: {PermissionId}", updatedPermission.Id);

                return permissionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error modifying permission with ID: {PermissionId}", request.Id);
                throw;
            }
        }
    }
}
