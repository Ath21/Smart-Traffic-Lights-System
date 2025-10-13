using System.Net;
using System.Text.Json;
using MongoDB.Driver;
using RabbitMQ.Client.Exceptions;
using LogData.Collections;
using LogData.Repositories.Error;
using MassTransit;
using MongoDB.Bson;

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
        catch (Exception ex)
        {
            // Classify and handle
            var (statusCode, userMessage, errorType) = ex switch
            {
                UnauthorizedAccessException      => (HttpStatusCode.Unauthorized, "Unauthorized access", "AUTH_ERROR"),
                KeyNotFoundException             => (HttpStatusCode.NotFound, "Resource not found", "NOT_FOUND"),
                InvalidOperationException        => (HttpStatusCode.BadRequest, "Invalid operation", "INVALID_OPERATION"),
                ArgumentNullException            => (HttpStatusCode.BadRequest, "Missing required parameter", "ARGUMENT_NULL"),
                ArgumentException                => (HttpStatusCode.BadRequest, "Invalid parameter", "ARGUMENT_ERROR"),
                FormatException                  => (HttpStatusCode.BadRequest, "Invalid data format", "FORMAT_ERROR"),
                JsonException                    => (HttpStatusCode.BadRequest, "Invalid JSON payload", "JSON_ERROR"),
                InvalidCastException             => (HttpStatusCode.BadRequest, "Invalid type conversion", "CAST_ERROR"),
                TimeoutException                 => (HttpStatusCode.RequestTimeout, "Operation timed out", "TIMEOUT"),
                HttpRequestException             => (HttpStatusCode.BadGateway, "External service unavailable", "HTTP_REQUEST_ERROR"),
                TaskCanceledException            => (HttpStatusCode.RequestTimeout, "Operation canceled or timed out", "TASK_CANCELED"),
                RequestTimeoutException          => (HttpStatusCode.GatewayTimeout, "Message broker timeout", "BROKER_TIMEOUT"),
                BrokerUnreachableException       => (HttpStatusCode.BadGateway, "Message broker unreachable", "BROKER_UNREACHABLE"),
                OperationInterruptedException    => (HttpStatusCode.ServiceUnavailable, "Message broker interrupted operation", "BROKER_INTERRUPTED"),
                MongoAuthenticationException     => (HttpStatusCode.Unauthorized, "MongoDB authentication failed", "DB_AUTH_ERROR"),
                MongoConnectionException         => (HttpStatusCode.ServiceUnavailable, "MongoDB connection failure", "DB_CONN_ERROR"),
                MongoWriteException              => (HttpStatusCode.Conflict, "MongoDB write conflict", "DB_WRITE_ERROR"),
                MongoException                   => (HttpStatusCode.InternalServerError, "MongoDB error", "DB_ERROR"),
                _                                => (HttpStatusCode.InternalServerError, "An unexpected error occurred", "UNEXPECTED")
            };

            await HandleErrorAsync(context, errorRepo, statusCode, userMessage, errorType, ex);
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
            // Create structured error document
            var metadata = new BsonDocument
            {
                { "Path", context.Request.Path.Value ?? "Unknown" },
                { "Method", context.Request.Method },
                { "TraceId", context.TraceIdentifier },
                { "ErrorType", errorType },
                { "ExceptionType", ex.GetType().FullName ?? "Unknown" },
                { "Message", ex.Message },
                { "StackTrace", ex.StackTrace ?? string.Empty },
                { "InnerException", ex.InnerException?.Message ?? string.Empty },
                { "Timestamp", DateTime.UtcNow }
            };

            var errorLog = new ErrorLogCollection
            {
                CorrelationId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Layer = "Log",
                Service = "Log Service",
                Action = context.Request.Path,
                Message = $"{userMessage}: {ex.Message}",
                Metadata = metadata
            };

            await errorRepo.InsertAsync(errorLog);

            _logger.LogInformation("[EXCEPTION] Stored error log ({ErrorType}) in MongoDB", errorType);
        }
        catch (Exception dbEx)
        {
            _logger.LogError(dbEx, "[EXCEPTION] Failed to write exception log to MongoDB");
        }

        // Response payload
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var responseBody = new
        {
            error = userMessage,
            details = ex.Message,
            type = errorType,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(responseBody));
    }
}
