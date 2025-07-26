using MediatR;
using Microsoft.Extensions.Logging;
using N5.Permissions.Application.DTOs;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Exceptions;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Application.Commands.RequestPermission
{
    public class RequestPermissionCommandHandler : IRequestHandler<RequestPermissionCommand, PermissionDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly IKafkaProducerService _kafkaService;

        public RequestPermissionCommandHandler(
            IUnitOfWork unitOfWork,
            IElasticsearchService elasticsearchService,
            IKafkaProducerService kafkaService)
        {
            _unitOfWork = unitOfWork;
            _elasticsearchService = elasticsearchService;
            _kafkaService = kafkaService;
        }

        public async Task<PermissionDto> Handle(RequestPermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Verify permission type exists
                var permissionType = await _unitOfWork.PermissionTypes.GetByIdAsync(request.PermissionTypeId);
                if (permissionType == null)
                {
                    throw new PermissionTypeNotFoundException(request.PermissionTypeId);
                }

                // Create permission entity
                var permission = new Permission
                {
                    EmployeeForename = request.EmployeeForename,
                    EmployeeSurname = request.EmployeeSurname,
                    PermissionTypeId = request.PermissionTypeId,
                    PermissionDate = request.PermissionDate,
                    CreatedAt = DateTime.UtcNow
                };

                // Save to database
                var savedPermission = await _unitOfWork.Permissions.AddAsync(permission);
                await _unitOfWork.SaveChangesAsync();

                // Create DTO for response
                var permissionDto = new PermissionDto
                {
                    Id = savedPermission.Id,
                    EmployeeForename = savedPermission.EmployeeForename,
                    EmployeeSurname = savedPermission.EmployeeSurname,
                    PermissionTypeId = savedPermission.PermissionTypeId,
                    PermissionTypeDescription = permissionType.Description,
                    PermissionDate = savedPermission.PermissionDate,
                    CreatedAt = savedPermission.CreatedAt,
                    UpdatedAt = savedPermission.UpdatedAt
                };

                // Index in Elasticsearch
                await _elasticsearchService.IndexPermissionAsync(savedPermission);

                // Send message to Kafka
                await _kafkaService.SendMessageAsync(Guid.NewGuid(), "request");

                await _unitOfWork.CommitTransactionAsync();


                return permissionDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
