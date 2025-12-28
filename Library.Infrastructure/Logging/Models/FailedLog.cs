
namespace Library.Infrastructure.Logging.Models
{
    public class FailedLog
    {
        public Guid Guid { get; set; }
        public required string OriginalMessage { get; set; }
        public required string FailedMessage { get; set; }
        public string? StackTrace { get; set; }
        public required string ServiceName { get; set; }
        public required string Level { get; set; } = "Failed";
        public required DateTime CreatedAt { get; set; }
    }
}
