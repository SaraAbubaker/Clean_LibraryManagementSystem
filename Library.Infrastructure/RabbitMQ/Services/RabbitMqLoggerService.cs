
using Library.Infrastructure.RabbitMQ.Configuation;
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.DTOs;
using Microsoft.Extensions.Options;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;

namespace Library.Infrastructure.RabbitMQ.Services
{
    public class RabbitMqLoggerService : IMessageLoggerService, IExceptionLoggerService
    {
        private readonly IConnection _connection;
        private readonly RabbitMqSettings _settings;

        public RabbitMqLoggerService(IConnection connection, IOptions<RabbitMqSettings> options)
        {
            _connection = connection;
            _settings = options.Value;
        }

        // IMessageLoggerService implementation
        public async Task LogInfoAsync(MessageLogDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            await PublishAsync(_settings.MessageQueue, json);
        }

        // IExceptionLoggerService implementation
        public async Task LogWarningAsync(WarningLogDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            await PublishAsync(_settings.ExceptionQueue, json);
        }

        public async Task LogExceptionAsync(ExceptionLogDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            await PublishAsync(_settings.ExceptionQueue, json);
        }

        // Shared publishing logic
        private async Task PublishAsync(string queueName, string message)
        {
            await using var channel = await _connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                body: body
            );
        }
    }
}
