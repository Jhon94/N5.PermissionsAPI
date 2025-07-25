using System.ComponentModel.DataAnnotations;

namespace N5.Permissions.Application.DTOs
{
    public class RequestPermissionDto
    {
        [Required]
        [MaxLength(100)]
        public string EmployeeForename { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EmployeeSurname { get; set; } = string.Empty;

        [Required]
        public int PermissionTypeId { get; set; }

        [Required]
        public DateTime PermissionDate { get; set; }
    }
}
