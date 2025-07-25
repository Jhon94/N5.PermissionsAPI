using System.ComponentModel.DataAnnotations;

namespace N5.Permissions.Domain.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}