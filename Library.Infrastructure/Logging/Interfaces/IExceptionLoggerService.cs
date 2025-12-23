using Library.Infrastructure.Logging.DTOs;
using Library.Infrastructure.Logging.Models;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IExceptionLoggerService
    {
        Task LogWarningAsync(WarningLogDto dto);
        Task LogExceptionAsync(ExceptionLogDto dto);
        Task<ExceptionLog?> GetExceptionLogAsync(Guid guid);
        Task<List<ExceptionLog>> GetAllExceptionLogsAsync();
    }
}
