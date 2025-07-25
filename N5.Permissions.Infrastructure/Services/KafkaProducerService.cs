using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using N5.Permissions.Application.DTOs;
using N5.Permissions.Domain.Interfaces;
using System.Text.Json;

namespace N5.Permissions.Infrastructure.Services
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly string _topicName;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            _topicName = configuration["KafkaSettings:Topic"] ?? "permissions-operations";

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["KafkaSettings:BootstrapServers"] ?? "localhost:9092",
                Acks = Acks.All,
                MessageSendMaxRetries = 3,
                EnableIdempotence = true,
                MessageTimeoutMs = 30000
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task SendMessageAsync(Guid id, string operation)
        {
            var message = new KafkaMessageDto
            {
                Id = id,
                NameOperation = operation,
                Timestamp = DateTime.UtcNow
            };

            await SendMessageAsync(message);
        }

        public async Task SendMessageAsync<T>(T message) where T : class
        {
            try
            {
                var messageJson = JsonSerializer.Serialize(message);
                var kafkaMessage = new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = messageJson,
                    Timestamp = new Timestamp(DateTime.UtcNow)
                };

                var deliveryResult = await _producer.ProduceAsync(_topicName, kafkaMessage);

                _logger.LogInformation("Message delivered to {Topic} partition {Partition} at offset {Offset}",
                    deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to deliver message to Kafka: {Error}", ex.Error.Reason);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to Kafka");
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
