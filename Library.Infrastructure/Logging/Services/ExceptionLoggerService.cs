using Library.Common.Helpers;
using Library.Common.RabbitMqMessages.LoggingMessages;
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.Models;
using Library.Infrastructure.Mongo;

namespace Library.Infrastructure.Logging.Services
{
    public class ExceptionLoggerService : IExceptionLoggerService
    {
        private readonly MongoRepository<ExceptionLog> _repo;

        public ExceptionLoggerService(MongoContext context)
        {
            _repo = new MongoRepository<ExceptionLog>(context, "ExceptionLogs");
        }

        public async Task LogWarningAsync(WarningLogMessage dto)
        {
            var log = new ExceptionLog
            {
                Guid = dto.Guid,
                CreatedAt = dto.CreatedAt,
                Level = dto.Level,
                ServiceName = dto.ServiceName,
                Request = dto.Request,
                WarningMessage = dto.WarningMessage,
                Response = dto.Response
            };

            Validate.ValidateModel(log);
            await _repo.InsertAsync(log);
        }

        public async Task LogExceptionAsync(ExceptionLogMessage dto)
        {
            var log = new ExceptionLog
            {
                Guid = dto.Guid,
                CreatedAt = dto.CreatedAt,
                Level = dto.Level,
                ServiceName = dto.ServiceName,
                Request = dto.Request,
                ExceptionMessage = dto.ExceptionMessage,
                StackTrace = dto.StackTrace
            };

            Validate.ValidateModel(log);
            await _repo.InsertAsync(log);
        }
    }
}