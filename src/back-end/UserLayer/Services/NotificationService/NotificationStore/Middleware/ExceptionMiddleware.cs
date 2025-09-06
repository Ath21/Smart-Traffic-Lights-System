using System.Net;
using System.Text.Json;
using MassTransit;
using MongoDB.Driver;
using RabbitMQ.Client.Exceptions;
using MailKit.Net.Smtp;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    private const string ServiceTag = "[" + nameof(ExceptionMiddleware) + "]";

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
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid argument provided", "ARGUMENT_ERROR", ex);
        }
        catch (FormatException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid data format", "FORMAT_ERROR", ex);
        }
        catch (JsonException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid JSON payload", "JSON_ERROR", ex);
        }

        // ============================
        // NETWORK / BROKER
        // ============================
        catch (TaskCanceledException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Request canceled or timed out", "TASK_CANCELED", ex);
        }
        catch (RequestTimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.GatewayTimeout, "Message broker timeout", "BROKER_TIMEOUT", ex);
        }
        catch (BrokerUnreachableException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Message broker unreachable", "BROKER_UNREACHABLE", ex);
        }
        catch (OperationInterruptedException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.ServiceUnavailable, "Message broker operation interrupted", "BROKER_INTERRUPTED", ex);
        }

        // ============================
        // EMAIL (MailKit / SMTP)
        // ============================
        catch (SmtpCommandException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "SMTP command failed", "SMTP_COMMAND_ERROR", ex);
        }
        catch (SmtpProtocolException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "SMTP protocol error", "SMTP_PROTOCOL_ERROR", ex);
        }

        // ============================
        // DATABASE (MongoDB)
        // ============================
        catch (MongoAuthenticationException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Unauthorized, "MongoDB authentication failed", "MONGO_AUTH", ex);
        }
        catch (MongoConnectionException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.ServiceUnavailable, "MongoDB connection error", "MONGO_CONNECTION", ex);
        }
        catch (MongoWriteException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Conflict, "MongoDB write error", "MONGO_WRITE", ex);
        }
        catch (MongoException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "MongoDB error", "MONGO_ERROR", ex);
        }

        // ============================
        // FALLBACK
        // ============================
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
            var publisher = context.RequestServices.GetRequiredService<INotificationLogPublisher>();

            await publisher.PublishErrorLogAsync(
                errorType,
                $"{ServiceTag} {ex.Message}",
                new
                {
                    Path = context.Request.Path,
                    Method = context.Request.Method,
                    TraceId = context.TraceIdentifier,
                    Exception = ex.GetType().Name,
                    StackTrace = ex.StackTrace
                },
                ex
            );

            _logger.LogInformation("{Tag} Error published to log exchange: {ErrorType}", ServiceTag, errorType);
        }
        catch (Exception pubEx)
        {
            _logger.LogError(pubEx, "{Tag} Failed to publish error log to broker", ServiceTag);
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
