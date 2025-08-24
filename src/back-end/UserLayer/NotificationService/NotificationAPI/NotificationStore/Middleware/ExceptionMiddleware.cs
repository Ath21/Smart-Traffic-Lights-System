using System.Net;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NotificationStore.Publishers;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        // 🔹 Authentication / Authorization
        catch (UnauthorizedAccessException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Unauthorized, "Unauthorized access", "AUTH_ERROR", ex);
        }

        // 🔹 Not found / bad usage
        catch (KeyNotFoundException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.NotFound, "Resource not found", "NOT_FOUND", ex);
        }
        catch (InvalidOperationException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Conflict, "Invalid operation", "INVALID_OPERATION", ex);
        }
        catch (ArgumentNullException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Missing required parameter", "ARGUMENT_NULL", ex);
        }
        catch (ArgumentException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid argument provided", "ARGUMENT_ERROR", ex);
        }
        catch (FormatException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid data format", "FORMAT_ERROR", ex);
        }

        // 🔹 Network / Messaging
        catch (TaskCanceledException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Request canceled or timed out", "TASK_CANCELED", ex);
        }
        catch (MassTransit.RequestTimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Message broker timeout", "BROKER_TIMEOUT", ex);
        }

        // 🔹 MongoDB
        catch (MongoWriteException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Conflict, "Mongo write error", "MONGO_WRITE", ex);
        }
        catch (MongoConnectionException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.ServiceUnavailable, "Mongo connection error", "MONGO_CONNECTION", ex);
        }

        // 🔹 Fallback
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
        _logger.LogError(ex, "{Message}", userMessage);

        try
        {
            // publish structured log
            var publisher = context.RequestServices.GetRequiredService<ILogPublisher>();
            await publisher.PublishErrorLogAsync(
                errorType,
                ex.Message,
                new
                {
                    Path = context.Request.Path,
                    Method = context.Request.Method,
                    TraceId = context.TraceIdentifier
                },
                ex
            );
        }
        catch (Exception pubEx)
        {
            _logger.LogError(pubEx, "Failed to publish error log to broker");
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
