using Library.Infrastructure.Logging.DTOs;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IExceptionLoggerService
    {
        Task LogWarningAsync(WarningLogDto dto);
        Task LogExceptionAsync(ExceptionLogDto dto);
    }
}
