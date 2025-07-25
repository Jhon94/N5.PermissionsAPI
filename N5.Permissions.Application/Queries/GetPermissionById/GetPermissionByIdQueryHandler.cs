using MediatR;
using Microsoft.Extensions.Logging;
using N5.Permissions.Application.DTOs;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Application.Queries.GetPermissionById
{
    public class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, PermissionDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetPermissionByIdQueryHandler> _logger;

        public GetPermissionByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetPermissionByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PermissionDto?> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting permission by ID: {PermissionId}", request.Id);

            var permission = await _unitOfWork.Permissions.GetByIdAsync(request.Id);

            if (permission == null)
            {
                _logger.LogWarning("Permission with ID {PermissionId} not found", request.Id);
                return null;
            }

            return new PermissionDto
            {
                Id = permission.Id,
                EmployeeForename = permission.EmployeeForename,
                EmployeeSurname = permission.EmployeeSurname,
                PermissionTypeId = permission.PermissionTypeId,
                PermissionTypeDescription = permission.PermissionType?.Description ?? "",
                PermissionDate = permission.PermissionDate,
                CreatedAt = permission.CreatedAt,
                UpdatedAt = permission.UpdatedAt
            };
        }
    }
}
