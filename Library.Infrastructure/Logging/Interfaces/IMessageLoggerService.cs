using Library.Infrastructure.Logging.DTOs;
using Library.Infrastructure.Logging.Models;

namespace Library.Infrastructure.Logging.Interfaces
{
    public interface IMessageLoggerService
    {
        Task LogInfoAsync(MessageLogDTO dto);
        Task<MessageLog?> GetMessageLogAsync(Guid guid);
        Task<List<MessageLog>> GetAllMessageLogsAsync();
    }
}
