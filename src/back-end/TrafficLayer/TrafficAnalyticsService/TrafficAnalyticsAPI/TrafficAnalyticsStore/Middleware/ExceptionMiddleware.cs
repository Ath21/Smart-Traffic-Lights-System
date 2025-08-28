using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using LogMessages;
using Microsoft.EntityFrameworkCore;
using TrafficAnalyticsStore.Publishers.Logs;

namespace TrafficAnalyticsStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

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
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.Unauthorized, "Unauthorized access", "AUTH_ERROR");
        }
        catch (KeyNotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound, "Resource not found", "NOT_FOUND");
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "Invalid operation", "INVALID_OPERATION");
        }
        catch (ArgumentNullException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "Missing required parameter", "ARGUMENT_NULL");
        }
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "Invalid parameter", "ARGUMENT_ERROR");
        }
        catch (FormatException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "Invalid data format", "FORMAT_ERROR");
        }
        catch (TimeoutException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.RequestTimeout, "Operation timed out", "TIMEOUT");
        }
        catch (HttpRequestException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadGateway, "External service unavailable", "HTTP_REQUEST_ERROR");
        }
        catch (TaskCanceledException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.RequestTimeout, "Operation canceled or timed out", "TASK_CANCELED");
        }
        catch (DbUpdateException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.Conflict, "Database update failed", "DB_UPDATE_ERROR");
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "An unexpected error occurred", "UNEXPECTED");
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception ex,
        HttpStatusCode statusCode,
        string userMessage,
        string errorType)
    {
        _logger.LogError(ex, "{ErrorType} exception caught by middleware", errorType);

        try
        {
            // resolve publisher from DI scope
            var publisher = context.RequestServices.GetRequiredService<IAnalyticsLogPublisher>();

            var errorLog = new ErrorLogMessage(
                Guid.NewGuid(),
                "TrafficAnalyticsService",
                errorType,
                ex.Message,
                DateTime.UtcNow,
                new { Path = context.Request.Path, Method = context.Request.Method, TraceId = context.TraceIdentifier }
            );

            await publisher.PublishErrorAsync(errorLog);
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
