using RabbitMQ.Client;
using System.Text;

namespace Library.Infrastructure.RabbitMQ
{
    public class LogPublisher
    {
        private readonly RabbitMqConnection _connection;

        public LogPublisher(RabbitMqConnection connection)
        {
            _connection = connection;
        }

        public async Task PublishMessageAsync(string message)
        {
            await PublishToQueueAsync(message, "message-logs");
        }

        public async Task PublishExceptionAsync(string exceptionMessage)
        {
            await PublishToQueueAsync(exceptionMessage, "exception-logs");
        }

        private async Task PublishToQueueAsync(string message, string queueName)
        {
            await using var connection = await _connection.GetConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

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
                ContentType = "text/plain",
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
