using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Data.Context;

namespace N5.Permissions.Infrastructure.Data.Repositories
{
    public class PermissionTypeRepository : BaseRepository<PermissionType>, IPermissionTypeRepository
    {
        public PermissionTypeRepository(PermissionsDbContext context) : base(context)
        {
        }
    }
}
