using Library.Infrastructure.Logging.DTOs;
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.Models;
using MassTransit;

namespace Library.API.Consuming
{
    public class MessageLogsConsumer : IConsumer<MessageLogDto>
    {
        private readonly IMessageLoggerService _messageLogger;
        private readonly IFailedLoggerService _failedLogger;

        public MessageLogsConsumer(IMessageLoggerService messageLogger, IFailedLoggerService failedLogger)
        {
            _messageLogger = messageLogger;
            _failedLogger = failedLogger;
        }

        public async Task Consume(ConsumeContext<MessageLogDto> context)
        {
            try
            {
                // Normal path → log info message
                await _messageLogger.LogInfoAsync(context.Message);
            }
            catch (Exception ex)
            {
                // Fallback → log failed entry
                await LogFailedAsync("Message log failed", context.Message, nameof(MessageLogsConsumer), ex);
            }
        }

        private async Task LogFailedAsync(string reason, object originalMessage, string consumerName, Exception ex)
        {
            var failedDto = new FailedLogDto
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Level = MyLogLevel.Failed,
                ServiceName = consumerName,
                OriginalMessage = System.Text.Json.JsonSerializer.Serialize(originalMessage),
                FailedMessage = reason,
                StackTrace = ex.StackTrace ?? string.Empty
            };

            await _failedLogger.LogFailedAsync(failedDto);
        }
    }
}
