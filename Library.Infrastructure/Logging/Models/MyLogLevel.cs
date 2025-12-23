
using System.Globalization;

namespace Library.Infrastructure.Logging.Models
{
    public static class MyLogLevel
    {
        public const string Info = "Info";            // General informational messages,           ex: "Scheduled job completed"
        public const string Warning = "Warning";      // Unexpected situations that aren’t errors, ex: "Borrow limit exceeded"
        public const string Exception = "Exception";  // Actual errors or exceptions,              ex: "Null reference exception"
    }
}
