
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

        public async Task LogMessageAsync(
            string request,
            string? response = null,
            MyLogLevel level = MyLogLevel.Info,
            string? serviceName = null)
        {
            if (level != MyLogLevel.Info && level != MyLogLevel.Request)
            {
                return;
            }

            var log = new MessageLog
            {
                Guid = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Request = request,
                Response = response,
                Level = level,
                ServiceName = serviceName ?? "UnknownService"
            };

            Validate.ValidateModel(log);
            await _repo.InsertAsync(log);
        }

        public async Task<MessageLog?> GetMessageLogAsync(Guid guid)
        {
            return await _repo.FindOneAsync(x => x.Guid == guid);
        }

        public async Task<List<MessageLog>> GetAllMessageLogsAsync()
        {
            return await _repo.FindAsync(_ => true);
        }
    }
}
