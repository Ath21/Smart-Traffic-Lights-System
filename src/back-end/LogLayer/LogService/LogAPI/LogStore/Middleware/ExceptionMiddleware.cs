/*
 * LogStore.Middleware.ExceptionMiddleware
 *
 * This class is a middleware for handling exceptions in the ASP.NET Core application.
 * It catches unhandled exceptions, logs them, and returns a 500 Internal Server Error response
 * with a JSON error message.
 * The middleware is registered in the application's request pipeline to ensure that any exceptions
 * thrown during request processing are handled gracefully.
 */
namespace LogStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception occurred");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred" });
        }
    }
}
