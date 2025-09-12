using System.Net;
using System.Text.Json;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RabbitMQ.Client.Exceptions;
using TrafficLightCoordinatorStore.Publishers.Logs;

namespace TrafficLightCoordinatorStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    private const string ServiceTag = "[" + nameof(ExceptionMiddleware) + "]";

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            await HandleAsync(context, HttpStatusCode.Unauthorized, "Unauthorized access", "AUTH_ERROR", ex);
        }

        // ============================
        // CLIENT ERRORS
        // ============================
        catch (KeyNotFoundException ex)
        {
            await HandleAsync(context, HttpStatusCode.NotFound, "Resource not found", "NOT_FOUND", ex);
        }
        catch (ArgumentNullException ex)
        {
            await HandleAsync(context, HttpStatusCode.BadRequest, "Missing required parameter", "ARGUMENT_NULL", ex);
        }
        catch (ArgumentException ex)
        {
            await HandleAsync(context, HttpStatusCode.BadRequest, "Invalid parameter", "ARGUMENT_ERROR", ex);
        }
        catch (FormatException ex)
        {
            await HandleAsync(context, HttpStatusCode.BadRequest, "Invalid data format", "FORMAT_ERROR", ex);
        }
        catch (InvalidOperationException ex)
        {
            await HandleAsync(context, HttpStatusCode.BadRequest, "Invalid operation", "INVALID_OPERATION", ex);
        }
        catch (JsonException ex)
        {
            await HandleAsync(context, HttpStatusCode.BadRequest, "Invalid JSON payload", "JSON_ERROR", ex);
        }

        // ============================
        // NETWORK / BROKER
        // ============================
        catch (TimeoutException ex)
        {
            await HandleAsync(context, HttpStatusCode.RequestTimeout, "Operation timed out", "TIMEOUT", ex);
        }
        catch (RequestTimeoutException ex)
        {
            await HandleAsync(context, HttpStatusCode.GatewayTimeout, "Message broker timeout", "BROKER_TIMEOUT", ex);
        }
        catch (TaskCanceledException ex)
        {
            await HandleAsync(context, HttpStatusCode.RequestTimeout, "Operation canceled or timed out", "TASK_CANCELED", ex);
        }
        catch (HttpRequestException ex)
        {
            await HandleAsync(context, HttpStatusCode.BadGateway, "Upstream HTTP request failed", "HTTP_REQUEST_ERROR", ex);
        }
        catch (BrokerUnreachableException ex)
        {
            await HandleAsync(context, HttpStatusCode.BadGateway, "Message broker unreachable", "BROKER_UNREACHABLE", ex);
        }
        catch (OperationInterruptedException ex)
        {
            await HandleAsync(context, HttpStatusCode.ServiceUnavailable, "Broker operation interrupted", "BROKER_INTERRUPTED", ex);
        }
        catch (ConfigurationException ex) // MassTransit bus config
        {
            await HandleAsync(context, HttpStatusCode.BadGateway, "Message bus configuration error", "BROKER_CONFIG_ERROR", ex);
        }

        // ============================
        // DATABASE
        // ============================
        catch (DbUpdateConcurrencyException ex)
        {
            await HandleAsync(context, HttpStatusCode.Conflict, "Database concurrency conflict", "DB_CONCURRENCY_ERROR", ex);
        }
        catch (DbUpdateException ex)
        {
            await HandleAsync(context, HttpStatusCode.Conflict, "Database update failed", "DB_UPDATE_ERROR", ex);
        }
        catch (SqlException ex) // MSSQL-specific errors
        {
            await HandleAsync(context, HttpStatusCode.BadGateway, "SQL Server database error", "DB_MSSQL_ERROR", ex);
        }


        // ============================
        // FALLBACK
        // ============================
        catch (Exception ex)
        {
            await HandleAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred", "UNEXPECTED", ex);
        }
    }

    private async Task HandleAsync(
        HttpContext ctx,
        HttpStatusCode status,
        string userMessage,
        string errorType,
        Exception ex)
    {
        _logger.LogError(ex, "{Tag} {Message}", ServiceTag, userMessage);

        var publisher = ctx.RequestServices.GetService<ITrafficLogPublisher>();
        if (publisher != null)
        {
            var metadata = new
            {
                serviceTag = ServiceTag,
                path = ctx.Request?.Path.Value,
                method = ctx.Request?.Method,
                traceId = ctx.TraceIdentifier,
                status = (int)status,
                query = ctx.Request?.QueryString.Value
            };

            try
            {
                await publisher.PublishErrorAsync(
                    errorType: errorType,
                    message: $"{ServiceTag} {ex.Message}",
                    metadata: metadata,
                    ct: ctx.RequestAborted
                );

                _logger.LogInformation("{Tag} Error published to log exchange: {ErrorType}", ServiceTag, errorType);
            }
            catch (Exception pubEx)
            {
                _logger.LogWarning(pubEx, "{Tag} Failed to publish error log to broker", ServiceTag);
            }
        }

        if (!ctx.Response.HasStarted)
        {
            ctx.Response.StatusCode = (int)status;
            ctx.Response.ContentType = "application/json";

            await ctx.Response.WriteAsJsonAsync(new
            {
                error = userMessage,
                details = ex.Message,
                traceId = ctx.TraceIdentifier
            });
        }
    }
}
