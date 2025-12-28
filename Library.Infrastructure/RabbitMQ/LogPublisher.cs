using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace Library.Infrastructure.RabbitMQ
{
    public class LogPublisher
    {
        private readonly IConnection _connection;
        private readonly RabbitMqSettings _settings;

        public LogPublisher(IConnection connection, IOptions<RabbitMqSettings> options)
        {
            _connection = connection;
            _settings = options.Value;
        }

        public async Task PublishMessageAsync(string message)
        {
            await PublishToQueueAsync(message, _settings.MessageQueue);
        }

        public async Task PublishExceptionAsync(string exceptionMessage)
        {
            await PublishToQueueAsync(exceptionMessage, _settings.ExceptionQueue);
        }

        public async Task PublishFailedAsync(string failedMessage)
        {
            await PublishToQueueAsync(failedMessage, _settings.FailedQueue);
        }


        private async Task PublishToQueueAsync(string message, string queueName)
        {
            await using var channel = await _connection.CreateChannelAsync();

            // Ensure the queue exists
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var body = Encoding.UTF8.GetBytes(message);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: default
            );
        }
    }
}
