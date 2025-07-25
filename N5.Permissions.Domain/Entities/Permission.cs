using System.ComponentModel.DataAnnotations;

namespace N5.Permissions.Domain.Entities
{
    public class Permission : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string EmployeeForename { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EmployeeSurname { get; set; } = string.Empty;

        public int PermissionTypeId { get; set; }
        public virtual PermissionType PermissionType { get; set; } = null!;

        [Required]
        public DateTime PermissionDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Domain methods
        public void UpdatePermission(string employeeForename, string employeeSurname, int permissionTypeId, DateTime permissionDate)
        {
            EmployeeForename = employeeForename;
            EmployeeSurname = employeeSurname;
            PermissionTypeId = permissionTypeId;
            PermissionDate = permissionDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public string GetFullName() => $"{EmployeeForename} {EmployeeSurname}";
    }
}
