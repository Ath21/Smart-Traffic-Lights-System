using System.Net;
using System.Text.Json;
using MassTransit;
using MongoDB.Driver;
using RabbitMQ.Client.Exceptions;
using LogStore.Business;
using LogStore.Models.Dtos;

namespace LogStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ILogService logService)
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
            await HandleErrorAsync(context, HttpStatusCode.Unauthorized, "Unauthorized access", "AUTH_ERROR", ex, logService);
        }

        // ============================
        // CLIENT ERRORS
        // ============================
        catch (KeyNotFoundException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.NotFound, "Resource not found", "NOT_FOUND", ex, logService);
        }
        catch (InvalidOperationException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid operation", "INVALID_OPERATION", ex, logService);
        }
        catch (ArgumentNullException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Missing required parameter", "ARGUMENT_NULL", ex, logService);
        }
        catch (ArgumentException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid parameter", "ARGUMENT_ERROR", ex, logService);
        }
        catch (FormatException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid data format", "FORMAT_ERROR", ex, logService);
        }
        catch (JsonException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid JSON payload", "JSON_ERROR", ex, logService);
        }
        catch (InvalidCastException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid type conversion", "CAST_ERROR", ex, logService);
        }

        // ============================
        // NETWORK / TIMEOUTS
        // ============================
        catch (TimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Operation timed out", "TIMEOUT", ex, logService);
        }
        catch (HttpRequestException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "External service unavailable", "HTTP_REQUEST_ERROR", ex, logService);
        }
        catch (TaskCanceledException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Operation canceled or timed out", "TASK_CANCELED", ex, logService);
        }
        catch (RequestTimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.GatewayTimeout, "Message broker timeout", "BROKER_TIMEOUT", ex, logService);
        }
        catch (BrokerUnreachableException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Message broker unreachable", "BROKER_UNREACHABLE", ex, logService);
        }
        catch (OperationInterruptedException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.ServiceUnavailable, "Message broker interrupted operation", "BROKER_INTERRUPTED", ex, logService);
        }

        // ============================
        // DATABASE (MongoDB)
        // ============================
        catch (MongoAuthenticationException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Unauthorized, "MongoDB authentication failed", "DB_AUTH_ERROR", ex, logService);
        }
        catch (MongoConnectionException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.ServiceUnavailable, "MongoDB connection failure", "DB_CONN_ERROR", ex, logService);
        }
        catch (MongoWriteException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Conflict, "MongoDB write conflict", "DB_WRITE_ERROR", ex, logService);
        }
        catch (MongoException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "MongoDB error", "DB_ERROR", ex, logService);
        }

        // ============================
        // FALLBACK
        // ============================
        catch (Exception ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred", "UNEXPECTED", ex, logService);
        }
    }

    private async Task HandleErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string userMessage,
        string errorType,
        Exception ex,
        ILogService logService)
    {
        _logger.LogError(ex, "[EXCEPTION] {Message}", userMessage);

        try
        {
            // Store directly in MongoDB via business service
            await logService.StoreErrorLogAsync(new ErrorLogDto
            {
                LogId = Guid.NewGuid(),
                ServiceName = "LogService",
                ErrorType = errorType,
                Message = $"[EXCEPTION] {ex.Message}",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["Path"] = context.Request.Path,
                    ["Method"] = context.Request.Method,
                    ["TraceId"] = context.TraceIdentifier,
                    ["Exception"] = ex.GetType().Name,
                    ["StackTrace"] = ex.StackTrace ?? ""
                }
            });

            _logger.LogInformation("[EXCEPTION] Error stored in error_logs collection: {ErrorType}", errorType);
        }
        catch (Exception dbEx)
        {
            _logger.LogError(dbEx, "[EXCEPTION] Failed to store error log in database");
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