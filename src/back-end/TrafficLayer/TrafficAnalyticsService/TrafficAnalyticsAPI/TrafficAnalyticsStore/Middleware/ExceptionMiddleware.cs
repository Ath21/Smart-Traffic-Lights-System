using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using LogMessages;
using TrafficAnalyticsStore.Publishers.Logs;
using Microsoft.EntityFrameworkCore;

namespace TrafficAnalyticsStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IAnalyticsLogPublisher _logPublisher;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IAnalyticsLogPublisher logPublisher)
    {
        _next = next;
        _logger = logger;
        _logPublisher = logPublisher;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound, "Resource not found", "NotFound");
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.Unauthorized, "Unauthorized access", "Unauthorized");
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.Conflict, "Conflict occurred", "Conflict");
        }
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "Invalid request parameters", "BadRequest");
        }
        catch (TimeoutException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.RequestTimeout, "Request timed out", "Timeout");
        }
        catch (OperationCanceledException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.RequestTimeout, "Operation cancelled", "Cancelled");
        }
        catch (DbUpdateException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "Database operation failed", "DatabaseError");
        }
        catch (HttpRequestException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadGateway, "External service unavailable", "HttpError");
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "An unexpected error occurred", "Unhandled");
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, HttpStatusCode statusCode, string message, string errorType)
    {
        // log locally
        _logger.LogError(ex, "{ErrorType} exception caught by middleware", errorType);

        // publish error log
        var errorLog = new ErrorLogMessage(
            Guid.NewGuid(),
            "TrafficDataAnalyticsService",
            errorType,
            ex.Message,
            DateTime.UtcNow,
            new { Path = context.Request.Path, Method = context.Request.Method, Trace = ex.StackTrace }
        );

        await _logPublisher.PublishErrorAsync(errorLog);

        // return to client
        await WriteErrorResponse(context, statusCode, message);
    }

    private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new { error = message };
        await context.Response.WriteAsJsonAsync(response);
    }
}
