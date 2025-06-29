/*
 * TrafficDataAnalyticsService.Middleware.ExceptionMiddleware
 *
 * Middleware για την καθολική διαχείριση εξαιρέσεων στο Traffic Data Analytics Service.
 * Καταγράφει εξαιρέσεις, αντιστοιχεί status codes σε γνωστά error types
 * και επιστρέφει JSON responses με κατάλληλα μηνύματα.
 */

using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TrafficDataAnalyticsService.Middleware;

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
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await WriteErrorResponse(context, HttpStatusCode.NotFound, "Resource not found");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "Unauthorized access");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");
            await WriteErrorResponse(context, HttpStatusCode.Conflict, "Conflict occurred");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request: invalid argument");
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, "Invalid request parameters");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new { error = message };
        await context.Response.WriteAsJsonAsync(response);
    }
}
