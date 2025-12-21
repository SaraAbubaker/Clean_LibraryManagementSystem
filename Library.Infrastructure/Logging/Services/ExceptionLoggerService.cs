
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

        public async Task LogExceptionAsync(Exception ex, string serviceName, MyLogLevel level = MyLogLevel.Exception)
        {
            if (level != MyLogLevel.Warning && level != MyLogLevel.Exception)
            {
                return;
            }

            var log = new ExceptionLog
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Level = level,
                ServiceName = serviceName,
                ExceptionMessage = ex.Message,
                StackTrace = ex.StackTrace ?? string.Empty
            };

            Validate.ValidateModel(log);
            await _repo.InsertAsync(log);
        }

        public async Task<ExceptionLog?> GetExceptionLogAsync(Guid guid)
        {
            return await _repo.FindOneAsync(x => x.Guid == guid);
        }

        public async Task<List<ExceptionLog>> GetAllExceptionLogsAsync()
        {
            return await _repo.FindAsync(_ => true);
        }
    }
}
