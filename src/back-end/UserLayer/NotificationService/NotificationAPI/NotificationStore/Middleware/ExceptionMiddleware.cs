/*
 * NotificationStore.Middleware.ExceptionMiddleware
 *
 * This file is part of the NotificationStore project, which defines the ExceptionMiddleware class.
 * The ExceptionMiddleware class is a custom middleware for handling exceptions in the NotificationStore API.
 * It intercepts HTTP requests and catches various types of exceptions, logging them and returning appropriate HTTP status codes and messages.
 * The middleware is designed to provide a consistent error handling mechanism across the API.
 */
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
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized access error occurred.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized access.");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Key not found error occurred.");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("Resource not found.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation error occurred.");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsync("Conflict occurred while processing the request.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument error occurred.");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid argument provided.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("An unexpected error occurred.");
        }
    }
}
