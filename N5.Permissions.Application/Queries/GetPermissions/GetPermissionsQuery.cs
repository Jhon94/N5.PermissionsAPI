using MediatR;
using N5.Permissions.Application.DTOs;

namespace N5.Permissions.Application.Queries.GetPermissions
{
    public class GetPermissionsQuery : IRequest<IEnumerable<PermissionDto>>
    {
        public string? EmployeeName { get; set; }
        public int? PermissionTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
