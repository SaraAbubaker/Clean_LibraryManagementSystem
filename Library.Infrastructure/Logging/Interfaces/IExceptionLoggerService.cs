using Library.Common.RabbitMqMessages.LoggingMessages;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IExceptionLoggerService
    {
        Task LogWarningAsync(WarningLogMessage dto);
        Task LogExceptionAsync(ExceptionLogMessage dto);
    }
}