namespace N5.Permissions.Application.DTOs
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string EmployeeForename { get; set; } = string.Empty;
        public string EmployeeSurname { get; set; } = string.Empty;
        public int PermissionTypeId { get; set; }
        public string PermissionTypeDescription { get; set; } = string.Empty;
        public DateTime PermissionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string FullName => $"{EmployeeForename} {EmployeeSurname}";
    }
}
