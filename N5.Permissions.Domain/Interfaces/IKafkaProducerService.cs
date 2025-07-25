namespace N5.Permissions.Domain.Interfaces
{
    public interface IKafkaProducerService
    {
        Task SendMessageAsync(Guid id, string operation);
        Task SendMessageAsync<T>(T message) where T : class;
    }
}
