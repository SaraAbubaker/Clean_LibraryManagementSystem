using Library.Infrastructure.Logging.DTOs;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IMessageLoggerService
    {
        Task LogInfoAsync(MessageLogDto dto);
    }
}
