using System.Net;
using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client.Exceptions;
using TrafficAnalyticsStore.Publishers.Logs;

namespace TrafficAnalyticsStore.Middleware;

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

        // ============================
        // AUTH / SECURITY
        // ============================
        catch (UnauthorizedAccessException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Unauthorized, "Unauthorized access", "AUTH_ERROR", ex);
        }

        // ============================
        // CLIENT ERRORS
        // ============================
        catch (KeyNotFoundException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.NotFound, "Resource not found", "NOT_FOUND", ex);
        }
        catch (InvalidOperationException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid operation", "INVALID_OPERATION", ex);
        }
        catch (ArgumentNullException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Missing required parameter", "ARGUMENT_NULL", ex);
        }
        catch (ArgumentException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid parameter", "ARGUMENT_ERROR", ex);
        }
        catch (FormatException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid data format", "FORMAT_ERROR", ex);
        }
        catch (JsonException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid JSON payload", "JSON_ERROR", ex);
        }
        catch (InvalidCastException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid type conversion", "CAST_ERROR", ex);
        }

        // ============================
        // NETWORK / BROKER
        // ============================
        catch (TimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Operation timed out", "TIMEOUT", ex);
        }
        catch (HttpRequestException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "External service unavailable", "HTTP_REQUEST_ERROR", ex);
        }
        catch (TaskCanceledException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Operation canceled or timed out", "TASK_CANCELED", ex);
        }
        catch (RequestTimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.GatewayTimeout, "Message broker timeout", "BROKER_TIMEOUT", ex);
        }
        catch (BrokerUnreachableException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Message broker unreachable", "BROKER_UNREACHABLE", ex);
        }
        catch (OperationInterruptedException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.ServiceUnavailable, "Message broker interrupted", "BROKER_INTERRUPTED", ex);
        }

        // ============================
        // DATABASE (EF Core)
        // ============================
        catch (DbUpdateConcurrencyException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Conflict, "Database concurrency conflict", "DB_CONCURRENCY_ERROR", ex);
        }
        catch (DbUpdateException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Conflict, "Database update failed", "DB_UPDATE_ERROR", ex);
        }

        // ============================
        // FALLBACK
        // ============================
        catch (Exception ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred", "UNEXPECTED", ex);
        }
    }

    private async Task HandleErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string userMessage,
        string errorType,
        Exception ex)
    {
        _logger.LogError(ex, "{Tag} {Message}", ServiceTag, userMessage);

        try
        {
            var publisher = context.RequestServices.GetRequiredService<IAnalyticsLogPublisher>();

            await publisher.PublishErrorAsync(new LogMessages.ErrorLogMessage(
                Guid.NewGuid(),
                "traffic_analytics_service",
                errorType,
                $"{ServiceTag} {ex.Message}",
                DateTime.UtcNow,
                new
                {
                    Path = context.Request.Path,
                    Method = context.Request.Method,
                    TraceId = context.TraceIdentifier,
                    Exception = ex.GetType().Name,
                    StackTrace = ex.StackTrace
                }
            ));

            _logger.LogInformation("{Tag} Error published to log exchange: {ErrorType}", ServiceTag, errorType);
        }
        catch (Exception pubEx)
        {
            _logger.LogError(pubEx, "{Tag} Failed to publish error log to broker", ServiceTag);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = userMessage,
            details = ex.Message,
            traceId = context.TraceIdentifier
        });
    }
}
