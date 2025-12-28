using Library.Infrastructure.Logging.DTOs;
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.Models;
using Library.Infrastructure.Mongo;
using Library.Shared.Helpers;

namespace Library.Infrastructure.Logging.Services
{
    public class MessageLoggerService : IMessageLoggerService
    {
        private readonly MongoRepository<MessageLog> _repo;

        public MessageLoggerService(MongoContext context)
        {
            _repo = new MongoRepository<MessageLog>(context, "MessageLogs");
        }

        public async Task LogInfoAsync(MessageLogDto dto)
        {
            var log = new MessageLog
            {
                Guid = dto.Guid,
                CreatedAt = dto.CreatedAt,
                Request = dto.Request,
                Response = dto.Response,
                Level = dto.Level,
                ServiceName = dto.ServiceName
            };

            Validate.ValidateModel(log);
            await _repo.InsertAsync(log);
        }
    }
}
