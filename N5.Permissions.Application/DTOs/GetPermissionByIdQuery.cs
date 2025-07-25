using MediatR;

namespace N5.Permissions.Application.DTOs
{
    public class GetPermissionByIdQuery : IRequest<PermissionDto?>
    {
        public int Id { get; set; }

        public GetPermissionByIdQuery(int id)
        {
            Id = id;
        }
    }
}
