using Library.Common.RabbitMqMessages.LoggingMessages;
using Library.Infrastructure.Logging.Interfaces;
using MassTransit;

namespace Library.API.Consumer
{
    public class ExceptionLogsConsumer :
        IConsumer<ExceptionLogMessage>,
        IConsumer<WarningLogMessage>
    {
        private readonly IExceptionLoggerService _logger;
        private readonly IFailedLoggerService _failedLogger;

        public ExceptionLogsConsumer(IExceptionLoggerService logger, IFailedLoggerService failedLogger)
        {
            _logger = logger;
            _failedLogger = failedLogger;
        }

        public async Task Consume(ConsumeContext<ExceptionLogMessage> context)
        {
            try
            {
                await _logger.LogExceptionAsync(context.Message);
            }
            catch (Exception ex)
            {
                await LogFailedAsync("Exception log failed", context.Message, nameof(ExceptionLogsConsumer), ex);
            }
        }

        public async Task Consume(ConsumeContext<WarningLogMessage> context)
        {
            try
            {
                await _logger.LogWarningAsync(context.Message);
            }
            catch (Exception ex)
            {
                await LogFailedAsync("Warning log failed", context.Message, nameof(ExceptionLogsConsumer), ex);
            }
        }

        private async Task LogFailedAsync(string reason, object originalMessage, string consumerName, Exception ex)
        {
            var failedDto = new FailedLogMessage
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
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