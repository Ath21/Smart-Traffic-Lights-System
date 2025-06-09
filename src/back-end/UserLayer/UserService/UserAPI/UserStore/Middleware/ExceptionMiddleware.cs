/*
 * UserStore.Middleware.ExceptionMiddleware
 *
 * This class is part of the UserStore project, which is responsible for managing user-related operations
 * and services. The ExceptionMiddleware class is a custom middleware component that handles exceptions
 * that occur during the processing of HTTP requests in the UserStore application. It is designed to
 * catch specific exceptions, log them, and return appropriate HTTP responses to the client.
 * The middleware uses MassTransit to publish error logs to a message broker for further processing.
 * The middleware is typically registered in the ASP.NET Core pipeline to ensure that it is executed
 * for every incoming HTTP request.
 * The ExceptionMiddleware class contains the following key components:
 * - Constructor: Initializes the middleware with a request delegate and a logger.
 * - InvokeAsync: The main method that processes the HTTP request and handles exceptions.
 * - PublishError: A private method that publishes error logs to a message broker using MassTransit.
 * The middleware handles the following exceptions:
 * - UnauthorizedAccessException: Catches unauthorized access exceptions and returns a 401 Unauthorized response.
 * - KeyNotFoundException: Catches key not found exceptions and returns a 404 Not Found response.
 * - InvalidOperationException: Catches invalid operation exceptions and returns a 400 Bad Request response.
 * - Exception: Catches all other unhandled exceptions and returns a 500 Internal Server Error response.
 * The middleware also logs the exceptions using the provided logger and publishes error logs to a message broker
 * for further processing.
 * The ExceptionMiddleware class is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 */
using System.Net;
using UserStore.Messages;
using UserStore.Publishers;

namespace UserStore.Middleware;

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
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");

            var logPublisher = context.RequestServices.GetRequiredService<IUserLogPublisher>();
            //await logPublisher.PublishErrorAsync("Unauthorized access", ex);

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");

            var logPublisher = context.RequestServices.GetRequiredService<IUserLogPublisher>();
            //await logPublisher.PublishErrorAsync("Resource not found", ex);

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");

            var logPublisher = context.RequestServices.GetRequiredService<IUserLogPublisher>();
            //await logPublisher.PublishErrorAsync("Invalid operation", ex);

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            var logPublisher = context.RequestServices.GetRequiredService<IUserLogPublisher>();
            //await logPublisher.PublishErrorAsync("Unhandled exception", ex);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred" });
        }
    }
}
