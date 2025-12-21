
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.Models;

namespace Library.API.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMessageLoggerService _messageLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public LoggingMiddleware(
            RequestDelegate next,
            IMessageLoggerService messageLogger,
            IExceptionLoggerService exceptionLogger)
        {
            _next = next;
            _messageLogger = messageLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task Invoke(HttpContext context)
        {
            string serviceName = $"{context.Request.Method} {context.Request.Path}";

            //REQUEST
            await _messageLogger.LogMessageAsync(
                request: serviceName,
                level: MyLogLevel.Request,
                response: null,
                serviceName: serviceName
            );

            var originalBodyStream = context.Response.Body;
            await using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                //Read response body
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                string responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                //INFO
                await _messageLogger.LogMessageAsync(
                    request: serviceName,
                    level: MyLogLevel.Info,
                    response: $"Status: {context.Response.StatusCode}, Body: {responseText}",
                    serviceName: serviceName
                );

                //Copy response back to original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                //EXCEPTION
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    serviceName
                );

                context.Response.Body = originalBodyStream;
                throw;
            }
        }
    }
}
