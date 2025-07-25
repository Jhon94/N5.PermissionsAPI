using N5.Permissions.Domain.Entities;

namespace N5.Permissions.Domain.Interfaces
{
    public interface IPermissionRepository
    {
        Task<Permission?> GetByIdAsync(int id);
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<IEnumerable<Permission>> GetByEmployeeAsync(string employeeForename, string employeeSurname);
        Task<IEnumerable<Permission>> GetByPermissionTypeAsync(int permissionTypeId);
        Task<Permission> AddAsync(Permission permission);
        Task<Permission> UpdateAsync(Permission permission);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
