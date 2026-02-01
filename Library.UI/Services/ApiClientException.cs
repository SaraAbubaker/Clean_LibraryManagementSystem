
namespace Library.UI.Services
{
    public class ApiClientException : Exception
    {
        public System.Net.HttpStatusCode? StatusCode { get; }
        public string? Content { get; }

        public ApiClientException(string message, System.Net.HttpStatusCode? statusCode = null, string? content = null, Exception? inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
            Content = content;
        }

        public override string ToString()
            => $"{base.ToString()}{Environment.NewLine}StatusCode: {StatusCode}{Environment.NewLine}Content: {Content}";
    }
}