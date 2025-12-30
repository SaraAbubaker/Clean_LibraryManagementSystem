using Library.Infrastructure.Logging.DTOs;
using Library.Shared.Helpers;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Text;
using System.Text.Json;
using MassTransit;
using Library.Common.RabbitMqMessages.LoggingMessages;

namespace Library.API.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Resolve IPublishEndpoint per request scope
            var publishEndpoint = context.RequestServices.GetRequiredService<IPublishEndpoint>();

            // Capture Request
            context.Request.EnableBuffering();
            string requestBody = string.Empty;

            if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            var requestSummary = $"{context.Request.Method} {context.Request.Path}";

            // Capture Response
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

                // Exception → publish to ExceptionQueue
                var exceptionDto = new ExceptionLogDto
                {
                    Guid = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    Level = MyLogLevel.Exception,
                    ServiceName = serviceName,
                    Request = requestSummary + " " + requestBody,
                    ExceptionMessage = ex.Message,
                    StackTrace = (ex.StackTrace ?? string.Empty).TrimStart()
                };

                Validate.ValidateModel(exceptionDto);
                await publishEndpoint.Publish(exceptionDto);

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

                // Decide where to log
                if (!string.IsNullOrWhiteSpace(responseText) && responseText.Contains("\"success\""))
                {
                    if (responseText.Contains("\"success\":false"))
                    {
                        await PublishWarningAsync(publishEndpoint, serviceName, requestSummary, requestBody, responseText,
                            ExtractMessage(responseText));
                    }
                    else if (responseText.Contains("\"success\":true"))
                    {
                        // Info → publish to MessageQueue
                        var messageDto = new MessageLogDto
                        {
                            Guid = Guid.NewGuid(),
                            CreatedAt = DateTime.Now,
                            Level = MyLogLevel.Info,
                            ServiceName = serviceName,
                            Request = requestSummary + " " + requestBody,
                            Response = responseSummary
                        };

                        Validate.ValidateModel(messageDto);
                        await publishEndpoint.Publish(messageDto);
                    }
                    else
                    {
                        await PublishWarningAsync(publishEndpoint, serviceName, requestSummary, requestBody, responseSummary,
                            "Response did not contain a clear success flag.");
                    }
                }
                else
                {
                    // Non-JSON responses → Failed
                    try
                    {
                        JsonDocument.Parse(responseText); // will throw if invalid JSON

                        // If parsing succeeds but no "success" flag → Warning
                        await PublishWarningAsync(publishEndpoint, serviceName, requestSummary, requestBody, responseSummary,
                            "Response did not contain a clear success flag.");
                    }
                    catch (Exception ex)
                    {
                        var failedDto = new FailedLogDto
                        {
                            Guid = Guid.NewGuid(),
                            CreatedAt = DateTime.Now,
                            Level = MyLogLevel.Failed.ToString(),
                            ServiceName = serviceName,
                            OriginalMessage = requestSummary + " " + requestBody,
                            FailedMessage = "Response was not valid JSON.",
                            StackTrace = (ex.StackTrace ?? string.Empty).TrimStart()
                        };

                        Validate.ValidateModel(failedDto);
                        await publishEndpoint.Publish(failedDto);
                    }
                }

                // Restore response body
                await responseBody.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }
        }

        private async Task PublishWarningAsync(
            IPublishEndpoint publishEndpoint,
            string serviceName,
            string requestSummary,
            string requestBody,
            string responseText,
            string warningMessage)
        {
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

            Validate.ValidateModel(warningDto);
            await publishEndpoint.Publish(warningDto);
        }

        private static string ExtractMessage(string responseText)
        {
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
            return warningMessage;
        }
    }
}
