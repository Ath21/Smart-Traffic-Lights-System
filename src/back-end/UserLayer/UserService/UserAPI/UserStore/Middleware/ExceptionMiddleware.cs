using System;
using System.Net;
using MassTransit;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using UserStore.Messages;

namespace UserStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IPublishEndpoint publishEndpoint)
    {
        _next = next;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred" });
        }
    }

    private async Task PublishError(Exception ex)
    {
        try
        {
            var log = new LogError(
                ex.Message,
                DateTime.UtcNow,
                ex.StackTrace,
                DateTime.UtcNow
            );

            await _publishEndpoint.Publish(log);
        }
        catch (Exception publishEx)
        {
            _logger.LogError(publishEx, "Failed to publish error log to user.logs.error");
        }
    }
}
