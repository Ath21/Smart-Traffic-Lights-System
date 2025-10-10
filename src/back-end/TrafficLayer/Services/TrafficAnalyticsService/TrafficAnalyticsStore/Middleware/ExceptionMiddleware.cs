using System.Net;
using System.Text.Json;
using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client.Exceptions;
using AutoMapper;
using TrafficAnalyticsStore.Publishers.Logs;
using MassTransit;

namespace TrafficAnalyticsStore.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

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
        catch (SecurityTokenException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Unauthorized, "Invalid or expired token", "TOKEN_ERROR", ex);
        }
        catch (AuthenticationException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Unauthorized, "Authentication failed", "AUTHENTICATION_ERROR", ex);
        }

        // ============================
        // CLIENT / VALIDATION
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
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid argument", "ARGUMENT_ERROR", ex);
        }
        catch (FormatException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid data format", "FORMAT_ERROR", ex);
        }
        catch (JsonException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Invalid JSON payload", "JSON_ERROR", ex);
        }
        catch (AutoMapperMappingException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "Mapping error occurred", "MAPPING_ERROR", ex);
        }

        // ============================
        // DATABASE (EF Core)
        // ============================
        catch (DbUpdateConcurrencyException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Conflict, "Database concurrency conflict", "DB_CONCURRENCY_ERROR", ex);
        }
        catch (DbUpdateException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Conflict, "Database update failed", "DB_UPDATE_ERROR", ex);
        }

        // ============================
        // BROKER / NETWORK
        // ============================
        catch (TimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Operation timed out", "TIMEOUT", ex);
        }
        catch (RequestTimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.GatewayTimeout, "RabbitMQ request timeout", "BROKER_TIMEOUT", ex);
        }
        catch (BrokerUnreachableException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "RabbitMQ unreachable", "BROKER_UNREACHABLE", ex);
        }
        catch (OperationInterruptedException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.ServiceUnavailable, "RabbitMQ operation interrupted", "BROKER_INTERRUPTED", ex);
        }

        // ============================
        // FALLBACK
        // ============================
        catch (Exception ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "Unexpected error occurred", "UNEXPECTED", ex);
        }
    }

    private async Task HandleErrorAsync(
        HttpContext context,
        HttpStatusCode status,
        string userMessage,
        string errorType,
        Exception ex)
    {
        var correlationId = context.Request.Headers.ContainsKey("X-Correlation-ID")
            ? context.Request.Headers["X-Correlation-ID"].ToString()
            : Guid.NewGuid().ToString();

        _logger.LogError(ex, "[EXCEPTION] {ErrorType} - {Message}", errorType, userMessage);

        using var scope = _scopeFactory.CreateScope();
        var logPublisher = scope.ServiceProvider.GetRequiredService<ITrafficAnalyticsLogPublisher>();

        var metadata = new Dictionary<string, string>
        {
            ["path"] = context.Request.Path,
            ["method"] = context.Request.Method,
            ["trace_id"] = context.TraceIdentifier,
            ["correlation_id"] = correlationId,
            ["exception_type"] = ex.GetType().Name,
            ["exception_message"] = ex.Message
        };

        try
        {
            await logPublisher.PublishErrorAsync(
                action: errorType,
                errorMessage: $"[EXCEPTION] {ex.Message}",
                ex: ex,
                metadata: metadata,
                correlationId: Guid.Parse(correlationId));
        }
        catch (Exception pubEx)
        {
            _logger.LogError(pubEx, "[EXCEPTION] Failed to publish error log");
        }

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = userMessage,
            details = ex.Message,
            traceId = context.TraceIdentifier,
            correlationId
        });
    }
}