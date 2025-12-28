
using Library.Infrastructure.Logging.DTOs;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IFailedLoggerService
    {
        Task LogFailedAsync(FailedLogDto dto);
    }
}
