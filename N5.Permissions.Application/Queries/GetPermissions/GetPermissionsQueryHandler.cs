using MediatR;
using Microsoft.Extensions.Logging;
using N5.Permissions.Application.DTOs;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Application.Queries.GetPermissions
{
    public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, IEnumerable<PermissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IKafkaProducerService _kafkaService;
        private readonly ILogger<GetPermissionsQueryHandler> _logger;

        public GetPermissionsQueryHandler(
            IUnitOfWork unitOfWork,
            IKafkaProducerService kafkaService,
            ILogger<GetPermissionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _kafkaService = kafkaService;
            _logger = logger;
        }

        public async Task<IEnumerable<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing get permissions query");

            try
            {
                // Get permissions from database
                var permissions = await _unitOfWork.Permissions.GetAllAsync();

                // Apply filters if provided
                if (!string.IsNullOrEmpty(request.EmployeeName))
                {
                    permissions = permissions.Where(p =>
                        (p.EmployeeForename + " " + p.EmployeeSurname)
                        .Contains(request.EmployeeName, StringComparison.OrdinalIgnoreCase));
                }

                if (request.PermissionTypeId.HasValue)
                {
                    permissions = permissions.Where(p => p.PermissionTypeId == request.PermissionTypeId.Value);
                }

                if (request.FromDate.HasValue)
                {
                    permissions = permissions.Where(p => p.PermissionDate >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    permissions = permissions.Where(p => p.PermissionDate <= request.ToDate.Value);
                }

                // Convert to DTOs
                var permissionDtos = permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    EmployeeForename = p.EmployeeForename,
                    EmployeeSurname = p.EmployeeSurname,
                    PermissionTypeId = p.PermissionTypeId,
                    PermissionTypeDescription = p.PermissionType?.Description ?? "",
                    PermissionDate = p.PermissionDate,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                // Send message to Kafka
                await _kafkaService.SendMessageAsync(Guid.NewGuid(), "get");

                _logger.LogInformation("Retrieved {Count} permissions", permissionDtos.Count);

                return permissionDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions");
                throw;
            }
        }
    }
}
