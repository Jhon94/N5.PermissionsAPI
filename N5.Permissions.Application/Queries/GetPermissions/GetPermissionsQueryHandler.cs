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

        public GetPermissionsQueryHandler(
            IUnitOfWork unitOfWork,
            IKafkaProducerService kafkaService)
        {
            _unitOfWork = unitOfWork;
            _kafkaService = kafkaService;
        }

        public async Task<IEnumerable<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var permissions = await _unitOfWork.Permissions.GetAllAsync();

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

                await _kafkaService.SendMessageAsync(Guid.NewGuid(), "get");

                return permissionDtos;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
