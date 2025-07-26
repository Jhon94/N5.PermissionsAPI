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
        private readonly IProducer<string, string>? _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly string _topicName;
        private readonly bool _isAvailable;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
            _topicName = configuration["KafkaSettings:Topic"] ?? "permissions-operations";

            try
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = configuration["KafkaSettings:BootstrapServers"] ?? "localhost:9092",
                    Acks = Acks.All,
                    MessageSendMaxRetries = 3,
                    EnableIdempotence = true,
                    MessageTimeoutMs = 10000,
                    RequestTimeoutMs = 5000
                };

                _producer = new ProducerBuilder<string, string>(config).Build();
                _isAvailable = true;
                _logger.LogInformation("Kafka producer initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Kafka producer - will continue in mock mode");
                _producer = null;
                _isAvailable = false;
            }
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
                if (!_isAvailable || _producer == null)
                {
                    _logger.LogWarning("MOCK: Kafka not available - simulating message: {Message}",
                        JsonSerializer.Serialize(message));
                    return;
                }

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
                _logger.LogWarning(ex, "Failed to deliver message to Kafka - continuing without Kafka: {Error}", ex.Error.Reason);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error sending message to Kafka - continuing without Kafka");
            }
        }

        public void Dispose()
        {
            try
            {
                _producer?.Flush(TimeSpan.FromSeconds(5));
                _producer?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing Kafka producer");
            }
        }
    }
}