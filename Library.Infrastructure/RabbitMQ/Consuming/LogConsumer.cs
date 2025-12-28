
using Library.Infrastructure.RabbitMQ.Configuation;
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.DTOs;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using System.Text.Json;
using RabbitMQ.Client;
using System.Text;

namespace Library.Infrastructure.RabbitMQ.Consuming
{
    public class LogConsumer : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IMessageLoggerService _messageLogger;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IFailedLoggerService _failedLogger;
        private readonly RabbitMqSettings _settings;
        private readonly List<IChannel> _channels = new();

        public LogConsumer(
            IConnection connection,
            IMessageLoggerService messageLogger,
            IExceptionLoggerService exceptionLogger,
            IFailedLoggerService failedLogger,
            IOptions<RabbitMqSettings> options)
        {
            _connection = connection;
            _messageLogger = messageLogger;
            _exceptionLogger = exceptionLogger;
            _failedLogger = failedLogger;
            _settings = options.Value;
        }

        /// <summary>
        /// Start consuming from all queues: message, exception (warning+exception), failed.
        /// </summary>
        public async Task StartAllAsync()
        {
            await StartConsumingAsync(_settings.MessageQueue);
            await StartConsumingAsync(_settings.ExceptionQueue);
            await StartConsumingAsync(_settings.FailedQueue);
        }

        private async Task StartConsumingAsync(string queueName)
        {
            var channel = await _connection.CreateChannelAsync();
            _channels.Add(channel);

            await channel.QueueDeclareAsync(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine($"Received raw JSON from {queueName}: {message}");

                try
                {
                    using var doc = JsonDocument.Parse(message);

                    if (doc.RootElement.TryGetProperty("Level", out var levelProp))
                    {
                        var level = levelProp.GetString();

                        switch (level)
                        {
                            case "Info":
                                if (queueName == _settings.MessageQueue &&
                                    JsonSerializer.Deserialize<MessageLogDto>(message) is { } infoDto)
                                {
                                    infoDto.CreatedAt = DateTime.Now;
                                    await _messageLogger.LogInfoAsync(infoDto);
                                }
                                break;

                            case "Warning":
                                if (queueName == _settings.ExceptionQueue &&
                                    JsonSerializer.Deserialize<WarningLogDto>(message) is { } warningDto)
                                {
                                    warningDto.CreatedAt = DateTime.Now;
                                    await _exceptionLogger.LogWarningAsync(warningDto);
                                }
                                break;

                            case "Exception":
                                if (queueName == _settings.ExceptionQueue &&
                                    JsonSerializer.Deserialize<ExceptionLogDto>(message) is { } exceptionDto)
                                {
                                    exceptionDto.CreatedAt = DateTime.Now;
                                    await _exceptionLogger.LogExceptionAsync(exceptionDto);
                                }
                                break;

                            case "Failed":
                                if (queueName == _settings.FailedQueue &&
                                    JsonSerializer.Deserialize<FailedLogDto>(message) is { } failedDto)
                                {
                                    failedDto.CreatedAt = DateTime.Now;
                                    await _failedLogger.LogFailedAsync(failedDto);
                                }
                                break;

                            default:
                                Console.WriteLine($"Unknown log level: {level}");
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Missing 'Level' property in payload");
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            foreach (var channel in _channels)
                channel.Dispose();
        }
    }
}