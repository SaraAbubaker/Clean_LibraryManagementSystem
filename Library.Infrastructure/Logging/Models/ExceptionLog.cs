
using MongoDB.Bson.Serialization.Attributes;

namespace Library.Infrastructure.Logging.Models
{
    public class ExceptionLog
    {
        public Guid Guid { get; set; }
        public required string Request { get; set; }

        // For warnings
        [BsonIgnoreIfNull]
        public string? WarningMessage { get; set; }
        [BsonIgnoreIfNull]
        public string? Response { get; set; }

        // For exceptions
        [BsonIgnoreIfNull]
        public string? ExceptionMessage { get; set; }
        [BsonIgnoreIfNull]
        public string? StackTrace { get; set; }

        public required string ServiceName { get; set; }
        public required string Level { get; set; } // "Warning" or "Exception"
        public required DateTime CreatedAt { get; set; }
    }
}
