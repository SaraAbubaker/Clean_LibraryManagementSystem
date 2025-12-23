using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

public class LogConsumer
{
    private readonly IConnection _connection;

    public LogConsumer(IConnection connection)
    {
        _connection = connection;
    }

    public async Task StartConsumingAsync(string queueName)
    {
        // Create channel (async in v7)
        var channel = await _connection.CreateChannelAsync();

        // Declare queue (await the async call)
        await channel.QueueDeclareAsync(queue: queueName,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false);

        // Create async consumer
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received: {message}");

            // Return a Task to satisfy AsyncEventHandler
            await Task.CompletedTask;
        };

        // Start consuming (await the async call)
        await channel.BasicConsumeAsync(queue: queueName,
                                        autoAck: true,
                                        consumer: consumer);
    }
}
