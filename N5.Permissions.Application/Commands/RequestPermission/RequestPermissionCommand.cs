using MediatR;
using N5.Permissions.Application.DTOs;

namespace N5.Permissions.Application.Commands.RequestPermission
{
    public class RequestPermissionCommand : IRequest<PermissionDto>
    {
        public string EmployeeForename { get; set; } = string.Empty;
        public string EmployeeSurname { get; set; } = string.Empty;
        public int PermissionTypeId { get; set; }
        public DateTime PermissionDate { get; set; }
    }
}
