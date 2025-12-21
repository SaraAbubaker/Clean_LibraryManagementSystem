
namespace Library.Infrastructure.Logging.Models
{
    public enum MyLogLevel
    {
        Info,       // General informational messages,           ex: "Scheduled job completed"
        Request,    // Logs for incoming requests or actions,    ex: "User requested book X"
        Warning,    // Unexpected situations that aren’t errors, ex: "Borrow limit exceeded"
        Exception   // Actual errors or exceptions,              ex: "Null reference exception"
    }
}
