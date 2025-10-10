using System.Net;
using System.Text.Json;
using MongoDB.Driver;
using RabbitMQ.Client.Exceptions;
using LogData.Collections;
using LogData.Repositories.Error;
using MassTransit;

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

    public async Task InvokeAsync(HttpContext context, IErrorLogRepository errorRepo)
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
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.Unauthorized, "Unauthorized access", "AUTH_ERROR", ex);
        }

        // ============================
        // CLIENT ERRORS
        // ============================
        catch (KeyNotFoundException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.NotFound, "Resource not found", "NOT_FOUND", ex);
        }
        catch (InvalidOperationException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.BadRequest, "Invalid operation", "INVALID_OPERATION", ex);
        }
        catch (ArgumentNullException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.BadRequest, "Missing required parameter", "ARGUMENT_NULL", ex);
        }
        catch (ArgumentException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.BadRequest, "Invalid parameter", "ARGUMENT_ERROR", ex);
        }
        catch (FormatException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.BadRequest, "Invalid data format", "FORMAT_ERROR", ex);
        }
        catch (JsonException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.BadRequest, "Invalid JSON payload", "JSON_ERROR", ex);
        }
        catch (InvalidCastException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.BadRequest, "Invalid type conversion", "CAST_ERROR", ex);
        }

        // ============================
        // NETWORK / TIMEOUTS
        // ============================
        catch (TimeoutException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.RequestTimeout, "Operation timed out", "TIMEOUT", ex);
        }
        catch (HttpRequestException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.BadGateway, "External service unavailable", "HTTP_REQUEST_ERROR", ex);
        }
        catch (TaskCanceledException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.RequestTimeout, "Operation canceled or timed out", "TASK_CANCELED", ex);
        }
        catch (RequestTimeoutException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.GatewayTimeout, "Message broker timeout", "BROKER_TIMEOUT", ex);
        }
        catch (BrokerUnreachableException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.BadGateway, "Message broker unreachable", "BROKER_UNREACHABLE", ex);
        }
        catch (OperationInterruptedException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.ServiceUnavailable, "Message broker interrupted operation", "BROKER_INTERRUPTED", ex);
        }

        // ============================
        // DATABASE (MongoDB)
        // ============================
        catch (MongoAuthenticationException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.Unauthorized, "MongoDB authentication failed", "DB_AUTH_ERROR", ex);
        }
        catch (MongoConnectionException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.ServiceUnavailable, "MongoDB connection failure", "DB_CONN_ERROR", ex);
        }
        catch (MongoWriteException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.Conflict, "MongoDB write conflict", "DB_WRITE_ERROR", ex);
        }
        catch (MongoException ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.InternalServerError, "MongoDB error", "DB_ERROR", ex);
        }

        // ============================
        // FALLBACK
        // ============================
        catch (Exception ex)
        {
            await HandleErrorAsync(context, errorRepo, HttpStatusCode.InternalServerError, "An unexpected error occurred", "UNEXPECTED", ex);
        }
    }

    private async Task HandleErrorAsync(
        HttpContext context,
        IErrorLogRepository errorRepo,
        HttpStatusCode statusCode,
        string userMessage,
        string errorType,
        Exception ex)
    {
        _logger.LogError(ex, "[EXCEPTION] {Message}", userMessage);

        try
        {
            // Create and insert structured error document
            var log = new ErrorLogCollection
            {
                CorrelationId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Layer = "System",
                Service = "Log Service Middleware",
                IntersectionId = 0,
                IntersectionName = "N/A",
                ErrorType = errorType,
                Action = context.Request.Path,
                Message = $"{userMessage}: {ex.Message}",
                Metadata = new MongoDB.Bson.BsonDocument
                {
                    { "Path", context.Request.Path.Value ?? "Unknown" },
                    { "Method", context.Request.Method },
                    { "TraceId", context.TraceIdentifier },
                    { "Exception", ex.GetType().Name },
                    { "StackTrace", ex.StackTrace ?? string.Empty },
                    { "InnerException", ex.InnerException?.Message ?? string.Empty }
                }
            };

            await errorRepo.InsertAsync(log);

            _logger.LogInformation("[EXCEPTION] Stored error log ({ErrorType}) in MongoDB.", errorType);
        }
        catch (Exception dbEx)
        {
            _logger.LogError(dbEx, "[EXCEPTION] Failed to write exception log to MongoDB");
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
