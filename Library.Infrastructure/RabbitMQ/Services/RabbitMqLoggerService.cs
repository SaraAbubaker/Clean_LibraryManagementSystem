using Library.Infrastructure.RabbitMQ.Configuation;
using Library.Infrastructure.Logging.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;
using Library.Common.RabbitMqMessages.LoggingMessages;

namespace Library.Infrastructure.RabbitMQ.Services
{
    public class RabbitMqLoggerService : IMessageLoggerService, IExceptionLoggerService
    {
        private readonly IConnection _connection;
        private readonly RabbitMqSettings _settings;

        public RabbitMqLoggerService(
            IConnection connection,
            IOptions<RabbitMqSettings> options)
        {
            _connection = connection;
            _settings = options.Value;
        }

        public async Task LogInfoAsync(MessageLogMessage dto)
        {
            var json = JsonSerializer.Serialize(dto);
            await PublishAsync(_settings.MessageQueue, json);
        }

        public async Task LogWarningAsync(WarningLogMessage dto)
        {
            var json = JsonSerializer.Serialize(dto);
            await PublishAsync(_settings.ExceptionQueue, json); // confirm intent
        }

        public async Task LogExceptionAsync(ExceptionLogMessage dto)
        {
            var json = JsonSerializer.Serialize(dto);
            await PublishAsync(_settings.ExceptionQueue, json);
        }

        private Task PublishAsync(string queueName, string message)
        {
            using var channel = _connection.CreateModel();

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: null,
                body: body
            );

            return Task.CompletedTask;
        }
    }
}
