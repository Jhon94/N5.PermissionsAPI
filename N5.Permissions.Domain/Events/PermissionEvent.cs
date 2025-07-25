namespace N5.Permissions.Domain.Events
{
    public abstract class PermissionEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public int PermissionId { get; set; }
        public string Operation { get; set; } = string.Empty;
    }
}
