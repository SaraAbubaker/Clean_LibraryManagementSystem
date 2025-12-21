
namespace Library.Infrastructure.Logging.Models
{
    public class ExceptionLog
    {
        public Guid Guid { get; set; }
        public required string ExceptionMessage { get; set; }
        public required string StackTrace { get; set; }
        public required string ServiceName { get; set; }
        public MyLogLevel Level { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
