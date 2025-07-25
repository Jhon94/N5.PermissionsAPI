using System.ComponentModel.DataAnnotations;

namespace N5.Permissions.Domain.Entities
{
    public class PermissionType : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
