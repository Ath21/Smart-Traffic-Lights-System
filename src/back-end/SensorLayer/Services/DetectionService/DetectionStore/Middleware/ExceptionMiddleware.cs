using System.Net;
using System.Text.Json;
using DetectionStore.Publishers.Logs;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client.Exceptions;

namespace DetectionStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    private const string ServiceTag = "[" + nameof(ExceptionMiddleware) + "]";

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(context, ex);
        }
    }

    private async Task HandleErrorAsync(HttpContext context, Exception ex)
    {
        var (statusCode, userMessage, errorType) = MapException(ex);

        _logger.LogError(ex, "{Tag} {Message}", ServiceTag, userMessage);

        try
        {
            var publisher = context.RequestServices.GetRequiredService<IDetectionLogPublisher>();

            await publisher.PublishErrorAsync(
                errorType,
                $"{ServiceTag} {ex.Message}",
                new
                {
                    Path = context.Request.Path,
                    Method = context.Request.Method,
                    TraceId = context.TraceIdentifier,
                    Exception = ex.GetType().Name,
                    StackTrace = ex.StackTrace
                });
        }
        catch (Exception pubEx)
        {
            _logger.LogError(pubEx, "{Tag} Failed to publish error log", ServiceTag);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = userMessage,
            details = ex.Message,
            traceId = context.TraceIdentifier
        }));
    }

    private static (HttpStatusCode statusCode, string userMessage, string errorType) MapException(Exception ex) =>
        ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access", "AUTH_ERROR"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found", "NOT_FOUND"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid operation", "INVALID_OPERATION"),
            ArgumentNullException => (HttpStatusCode.BadRequest, "Missing required parameter", "ARGUMENT_NULL"),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid parameter", "ARGUMENT_ERROR"),
            FormatException => (HttpStatusCode.BadRequest, "Invalid data format", "FORMAT_ERROR"),
            JsonException => (HttpStatusCode.BadRequest, "Invalid JSON payload", "JSON_ERROR"),
            TimeoutException or TaskCanceledException => (HttpStatusCode.RequestTimeout, "Operation timed out", "TIMEOUT"),
            HttpRequestException => (HttpStatusCode.BadGateway, "External service unavailable", "HTTP_REQUEST_ERROR"),
            RequestTimeoutException => (HttpStatusCode.GatewayTimeout, "Message broker timeout", "BROKER_TIMEOUT"),
            BrokerUnreachableException => (HttpStatusCode.BadGateway, "Message broker unreachable", "BROKER_UNREACHABLE"),
            OperationInterruptedException => (HttpStatusCode.ServiceUnavailable, "Broker operation interrupted", "BROKER_INTERRUPTED"),
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "Database concurrency conflict", "DB_CONCURRENCY_ERROR"),
            DbUpdateException => (HttpStatusCode.Conflict, "Database update failed", "DB_UPDATE_ERROR"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred", "UNEXPECTED")
        };
}
