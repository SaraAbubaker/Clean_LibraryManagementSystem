
namespace Library.Infrastructure.Logging.Models
{
    public class MessageLog
    {
        public Guid Guid { get; set; }
        public required string Request { get; set; }
        public string? Response { get; set; }
        public required string ServiceName { get; set; }
        public required MyLogLevel Level { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
