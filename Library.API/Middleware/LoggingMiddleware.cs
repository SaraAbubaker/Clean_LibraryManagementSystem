using Library.Infrastructure.Logging.DTOs;
using Library.Infrastructure.Logging.Interfaces;
using Library.Infrastructure.Logging.Models;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Text;

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

        public async Task InvokeAsync(HttpContext context)
        {
            //Capture Request
            context.Request.EnableBuffering();

            string requestBody = string.Empty;
            if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    leaveOpen: true);

                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            var requestSummary = $"{context.Request.Method} {context.Request.Path}";

            //Capture Response
            var originalBody = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var endpoint = context.GetEndpoint();
                var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
                string serviceName = actionDescriptor?.ActionName ?? requestSummary;

                //Exception
                var exceptionDto = new ExceptionLogDto
                {
                    Guid = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Level = MyLogLevel.Exception,
                    ServiceName = serviceName,
                    Request = requestSummary + " " + requestBody,
                    ExceptionMessage = ex.Message,
                    StackTrace = (ex.StackTrace ?? string.Empty).TrimStart()
                };

                await _exceptionLogger.LogExceptionAsync(exceptionDto);
                throw; // rethrow so pipeline continues correctly
            }
            finally
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                var responseSummary = $"Status: {context.Response.StatusCode}, Body: {responseText}";

                var endpoint = context.GetEndpoint();
                var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
                string serviceName = actionDescriptor?.ActionName ?? requestSummary;

                //Decide where to log
                if (!string.IsNullOrWhiteSpace(responseText) && responseText.Contains("\"success\""))
                {
                    if (responseText.Contains("\"success\":false"))
                    {
                        // Extract message manually
                        string warningMessage = "Unknown API error";
                        int msgIndex = responseText.IndexOf("\"message\":");
                        if (msgIndex >= 0)
                        {
                            int start = responseText.IndexOf('"', msgIndex + 10) + 1;
                            int end = responseText.IndexOf('"', start);
                            if (start > 0 && end > start)
                            {
                                warningMessage = responseText.Substring(start, end - start);
                            }
                        }

                        //WarningLogDto
                        var warningDto = new WarningLogDto
                        {
                            Guid = Guid.NewGuid(),
                            CreatedAt = DateTime.Now,
                            Level = MyLogLevel.Warning,
                            ServiceName = serviceName,
                            Request = requestSummary + " " + requestBody,
                            WarningMessage = warningMessage,
                            Response = responseText
                        };

                        await _exceptionLogger.LogWarningAsync(warningDto);
                    }
                    else if (responseText.Contains("\"success\":true"))
                    {
                        //MessageLogDto
                        var messageDto = new MessageLogDTO
                        {
                            Guid = Guid.NewGuid(),
                            CreatedAt = DateTime.Now,
                            Level = MyLogLevel.Info,
                            ServiceName = serviceName,
                            Request = requestSummary + " " + requestBody,
                            Response = responseSummary
                        };

                        await _messageLogger.LogInfoAsync(messageDto);
                    }
                    else
                    {
                        //No clear success flag → Info
                        var messageDto = new MessageLogDTO
                        {
                            Guid = Guid.NewGuid(),
                            CreatedAt = DateTime.Now,
                            Level = MyLogLevel.Info,
                            ServiceName = serviceName,
                            Request = requestSummary + " " + requestBody,
                            Response = responseSummary
                        };

                        await _messageLogger.LogInfoAsync(messageDto);
                    }
                }
                else
                {
                    //Non-JSON responses → Info
                    var messageDto = new MessageLogDTO
                    {
                        Guid = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        Level = MyLogLevel.Info,
                        ServiceName = serviceName,
                        Request = requestSummary + " " + requestBody,
                        Response = responseSummary
                    };

                    await _messageLogger.LogInfoAsync(messageDto);
                }

                // Restore response body
                await responseBody.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }
        }

    }
}
