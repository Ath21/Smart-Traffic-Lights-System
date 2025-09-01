using System;
using System.Net;
using System.Threading.Tasks;
using IntersectionControlStore.Publishers.LogPub;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MassTransit;

namespace IntersectionControlStore.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.Unauthorized, "Unauthorized access", ex);
            }
            catch (KeyNotFoundException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.NotFound, "Resource not found", ex);
            }
            catch (InvalidOperationException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid operation", ex);
            }
            catch (ArgumentNullException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Missing required parameter", ex);
            }
            catch (ArgumentException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid parameter", ex);
            }
            catch (FormatException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid data format", ex);
            }
            catch (System.Text.Json.JsonException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Malformed JSON", ex);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Validation failed", ex);
            }
            catch (TimeoutException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Operation timed out", ex);
            }
            catch (HttpRequestException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Dependent service connection failed", ex);
            }
            catch (MassTransit.RequestTimeoutException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Message broker timeout", ex);
            }
            catch (MassTransit.RequestFaultException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Message broker request fault", ex);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "Database update error", ex);
            }
            catch (TaskCanceledException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Operation canceled or timed out", ex);
            }
            catch (NotSupportedException ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.NotImplemented, "Operation not supported", ex);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred", ex);
            }
        }

        private async Task HandleErrorAsync(HttpContext context, HttpStatusCode statusCode, string message, Exception ex)
        {
            _logger.LogError(ex, message);

            using var scope = _scopeFactory.CreateScope();
            var logPublisher = scope.ServiceProvider.GetRequiredService<ITrafficLogPublisher>();

            await logPublisher.PublishErrorLogAsync("IntersectionControlService", message, ex);

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = message, details = ex.Message });
        }
    }
}
