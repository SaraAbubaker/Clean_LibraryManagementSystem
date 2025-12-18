
using LogLevel = Library.Infrastructure.Logging.Models.LogLevel;
using Library.Infrastructure.Logging.Services;

namespace Library.Infrastructure.Logging.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MessageLoggerService _logger;

        public LoggingMiddleware(RequestDelegate next, MessageLoggerService logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            //'Request' level
            await _logger.LogMessageAsync(
                request: $"{context.Request.Method} {context.Request.Path}",
                level: LogLevel.Request,
                response: null,
                serviceName: "API" //fix
            );

            try
            {
                await _next(context);

                //'Info' level
                await _logger.LogMessageAsync(
                    request: $"{context.Request.Method} {context.Request.Path}",
                    level: LogLevel.Info,
                    response: $"Status: {context.Response.StatusCode}",
                    serviceName: "API" //fix
                );
            }
            catch (Exception ex)
            {
                //'Exception' level
                await _logger.LogMessageAsync(
                    request: $"{context.Request.Method} {context.Request.Path}",
                    level: LogLevel.Exception,
                    response: ex.Message,
                    serviceName: "API" //fix
                );

                throw;
            }
        }
    }
}
