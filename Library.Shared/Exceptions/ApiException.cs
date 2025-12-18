
namespace Library.Shared.Exceptions
{
    public abstract class ApiException : Exception
    {
        protected ApiException(string message) : base(message) { }
        protected ApiException(string message, Exception inner) : base(message, inner) { }
    }
}
