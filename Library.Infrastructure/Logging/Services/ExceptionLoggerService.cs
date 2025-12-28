using Library.Infrastructure.Logging.DTOs;
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.Models;
using Library.Infrastructure.Mongo;
using Library.Shared.Helpers;

namespace Library.Infrastructure.Logging.Services
{
    public class ExceptionLoggerService : IExceptionLoggerService
    {
        private readonly MongoRepository<ExceptionLog> _repo;

        public ExceptionLoggerService(MongoContext context)
        {
            _repo = new MongoRepository<ExceptionLog>(context, "ExceptionLogs");
        }

        public async Task LogWarningAsync(WarningLogDto dto)
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

        public async Task LogExceptionAsync(ExceptionLogDto dto)
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
