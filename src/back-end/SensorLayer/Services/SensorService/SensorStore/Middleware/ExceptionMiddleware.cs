using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using SensorStore.Publishers.Logs;

namespace SensorStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly ISensorLogPublisher _logPublisher;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        ISensorLogPublisher logPublisher)
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
        catch (Exception ex)
        {
            // Local log
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);

            // Try to extract intersection id if available (from route or query)
            var intersectionId = ExtractIntersectionId(context);

            // Publish to RabbitMQ
            await _logPublisher.PublishErrorAsync(
                intersectionId,
                errorType: ex.GetType().Name,
                message: ex.Message,
                metadata: new Dictionary<string, object?>
                {
                    ["path"] = context.Request.Path,
                    ["method"] = context.Request.Method,
                    ["stackTrace"] = ex.StackTrace
                }
            );

            // Return response
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                error = "Internal Server Error",
                message = ex.Message,
                traceId = context.TraceIdentifier
            });

            await context.Response.WriteAsync(result);
        }
    }

    private int ExtractIntersectionId(HttpContext context)
    {
        // Example: GET /api/sensors/{intersectionId}
        if (context.Request.RouteValues.TryGetValue("intersectionId", out var routeVal) &&
            int.TryParse(routeVal?.ToString(), out var id))
        {
            return id;
        }

        // Optional: look into query ?intersectionId=123
        if (context.Request.Query.TryGetValue("intersectionId", out var queryVal) &&
            int.TryParse(queryVal, out var qid))
        {
            return qid;
        }

        return -1; // default: unknown
    }
}
