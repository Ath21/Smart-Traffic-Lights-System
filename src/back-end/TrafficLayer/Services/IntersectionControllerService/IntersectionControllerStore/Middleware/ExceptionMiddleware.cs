using System.Net;
using System.Text.Json;
using IntersectionControllerStore.Publishers.LogPub;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace IntersectionControllerStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private const string ServiceTag = "[" + nameof(ExceptionMiddleware) + "]";

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
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
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Malformed JSON", "JSON_ERROR", ex);
        }

        // ============================
        // NETWORK / BROKER (MassTransit / RabbitMQ)
        // ============================
        catch (TaskCanceledException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Operation canceled or timed out", "TASK_CANCELED", ex);
        }
        catch (TimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Operation timed out", "TIMEOUT", ex);
        }
        catch (RequestTimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.GatewayTimeout, "Message broker timeout", "BROKER_TIMEOUT", ex);
        }
        catch (RequestFaultException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Message broker request fault", "BROKER_FAULT", ex);
        }
        catch (RabbitMqConnectionException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Message broker connection failed", "BROKER_CONNECTION", ex);
        }

        // ============================
        // DATABASE (EF Core / Redis)
        // ============================
        catch (DbUpdateException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "Database update error", "DB_UPDATE_ERROR", ex);
        }
        catch (RedisConnectionException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Redis connection error", "REDIS_CONNECTION", ex);
        }
        catch (RedisServerException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Redis server error", "REDIS_SERVER", ex);
        }

        // ============================
        // FALLBACK
        // ============================
        catch (NotSupportedException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.NotImplemented, "Operation not supported", "NOT_SUPPORTED", ex);
        }
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
            using var scope = _scopeFactory.CreateScope();
            var logPublisher = scope.ServiceProvider.GetRequiredService<ITrafficLogPublisher>();

            await logPublisher.PublishErrorAsync(
                serviceName: "IntersectionControllerService",
                errorType: errorType,
                message: $"{ServiceTag} {ex.Message}",
                metadata: new
                {
                    Path = context.Request.Path,
                    Method = context.Request.Method,
                    TraceId = context.TraceIdentifier,
                    Exception = ex.GetType().Name,
                    StackTrace = ex.StackTrace
                }
            );

            _logger.LogInformation("{Tag} Error published to log exchange: {ErrorType}", ServiceTag, errorType);
        }
        catch (Exception pubEx)
        {
            _logger.LogError(pubEx, "{Tag} Failed to publish error log", ServiceTag);
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
