using MediatR;
using Microsoft.Extensions.Logging;
using N5.Permissions.Application.DTOs;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.Application.Queries.GetPermissionById
{
    public class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, PermissionDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPermissionByIdQueryHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PermissionDto?> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(request.Id);

            if (permission == null)
            {
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
