using Microsoft.EntityFrameworkCore;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Data.Context;

namespace N5.Permissions.Infrastructure.Data.Repositories
{
    public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
    {
        public PermissionRepository(PermissionsDbContext context) : base(context)
        {
        }

        public override async Task<Permission?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.PermissionType)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public override async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.PermissionType)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetByEmployeeAsync(string employeeForename, string employeeSurname)
        {
            return await _dbSet
                .Include(p => p.PermissionType)
                .Where(p => p.EmployeeForename == employeeForename && p.EmployeeSurname == employeeSurname)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetByPermissionTypeAsync(int permissionTypeId)
        {
            return await _dbSet
                .Include(p => p.PermissionType)
                .Where(p => p.PermissionTypeId == permissionTypeId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
