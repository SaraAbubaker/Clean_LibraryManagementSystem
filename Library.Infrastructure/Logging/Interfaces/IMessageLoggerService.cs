
using Library.Infrastructure.Logging.Models;
using LogLevel = Library.Infrastructure.Logging.Models.LogLevel;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IMessageLoggerService
    {
        Task LogMessageAsync(
            string request,
            string? response = null,
            LogLevel level = LogLevel.Info,
            string? serviceName = null);
        Task<MessageLog?> GetMessageLogAsync(Guid guid);
        Task<List<MessageLog>> GetAllMessageLogsAsync();
    }
}
