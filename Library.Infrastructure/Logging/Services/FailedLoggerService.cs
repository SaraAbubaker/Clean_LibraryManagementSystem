using Library.Common.Helpers;
using Library.Common.RabbitMqMessages.LoggingMessages;
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.Models;
using Library.Infrastructure.Mongo;

namespace Library.Infrastructure.Logging.Services
{
    public class FailedLoggerService : IFailedLoggerService
    {
        private readonly MongoRepository<FailedLog> _repo;

        public FailedLoggerService(MongoContext context)
        {
            _repo = new MongoRepository<FailedLog>(context, "FailedLogs");
        }

        public async Task LogFailedAsync(FailedLogMessage dto)
        {
            var log = new FailedLog
            {
                Guid = dto.Guid,
                CreatedAt = dto.CreatedAt,
                OriginalMessage = dto.OriginalMessage,
                FailedMessage = dto.FailedMessage,
                StackTrace = dto.StackTrace ?? string.Empty,
                ServiceName = dto.ServiceName,
                Level = dto.Level
            };

            Validate.ValidateModel(log);
            await _repo.InsertAsync(log);
        }
    }
}