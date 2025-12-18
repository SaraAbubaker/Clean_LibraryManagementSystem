
using Library.Infrastructure.Logging.Models;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IExceptionLoggerService
    {
        Task LogExceptionAsync(Exception ex, string serviceName);
        Task<ExceptionLog?> GetExceptionLogAsync(Guid guid);
        Task<List<ExceptionLog>> GetAllExceptionLogsAsync();
    }
}
