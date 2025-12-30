using Library.Common.RabbitMqMessages.LoggingMessages;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IMessageLoggerService
    {
        Task LogInfoAsync(MessageLogMessage dto);
    }
}