namespace N5.Permissions.Application.DTOs
{
    public class KafkaMessageDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NameOperation { get; set; } = string.Empty; // "request", "modify", "get"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public object? AdditionalData { get; set; }
    }
}
