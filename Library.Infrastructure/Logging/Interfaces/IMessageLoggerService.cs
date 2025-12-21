
using Library.Infrastructure.Logging.Models;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IMessageLoggerService
    {
        Task LogMessageAsync(
            string request,
            string? response = null,
            MyLogLevel level = MyLogLevel.Info,
            string? serviceName = null);
        Task<MessageLog?> GetMessageLogAsync(Guid guid);
        Task<List<MessageLog>> GetAllMessageLogsAsync();
    }
}
