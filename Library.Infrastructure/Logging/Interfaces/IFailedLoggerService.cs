using Library.Common.RabbitMqMessages.LoggingMessages;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IFailedLoggerService
    {
        Task LogFailedAsync(FailedLogMessage dto);
    }
}