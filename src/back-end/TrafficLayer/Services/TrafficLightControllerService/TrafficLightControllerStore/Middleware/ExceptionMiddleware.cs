using System;
using System.Net;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using RabbitMQ.Client;
using Microsoft.EntityFrameworkCore;
using TrafficLightControllerStore.Publishers.Logs;
using TrafficLightControllerStore.Failover;

namespace TrafficLightControllerStore.Middleware;

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
        // ========== Security ==========
        catch (UnauthorizedAccessException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Unauthorized, "Security.Unauthorized", ex);
        }

        // ========== Validation / Input ==========
        catch (ArgumentNullException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Validation.ArgumentNull", ex);
        }
        catch (ArgumentException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Validation.Argument", ex);
        }
        catch (FormatException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Validation.Format", ex);
        }
        catch (System.Text.Json.JsonException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Validation.JsonParse", ex);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Validation.DataAnnotations", ex);
        }

        // ========== Resource / Not Found ==========
        catch (KeyNotFoundException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.NotFound, "Resource.NotFound", ex);
        }

        // ========== Infrastructure: Redis ==========
        catch (RedisConnectionException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.ServiceUnavailable, "Infrastructure.Redis.Connection", ex, fallback: true);
        }
        catch (RedisTimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Infrastructure.Redis.Timeout", ex, fallback: true);
        }

        // ========== Infrastructure: RabbitMQ / MassTransit ==========
        catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.ServiceUnavailable, "Infrastructure.RabbitMQ.BrokerUnreachable", ex, fallback: true);
        }
        catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Infrastructure.RabbitMQ.OperationInterrupted", ex, fallback: true);
        }

        // ========== Infrastructure: Database (EF Core, SQL) ==========
        catch (DbUpdateException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Infrastructure.Database.Update", ex);
        }
        catch (InvalidOperationException ex) when (ex.Source?.Contains("Microsoft.EntityFrameworkCore") == true)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Infrastructure.Database.InvalidOperation", ex);
        }

        // ========== Infrastructure: HTTP / External Calls ==========
        catch (TimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Infrastructure.Timeout", ex, fallback: true);
        }
        catch (HttpRequestException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "Infrastructure.HttpRequest", ex, fallback: true);
        }
        catch (TaskCanceledException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Infrastructure.TaskCanceled", ex, fallback: true);
        }

        // ========== Not Implemented ==========
        catch (NotSupportedException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.NotImplemented, "General.NotSupported", ex);
        }

        // ========== Catch-All ==========
        catch (Exception ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "General.Unhandled", ex, fallback: true);
        }
    }

    private async Task HandleErrorAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string errorType,
        Exception ex,
        bool fallback = false)
    {
        _logger.LogError(ex, "[{Type}] {Message}", errorType, ex.Message);

        using var scope = _scopeFactory.CreateScope();
        var logPublisher = scope.ServiceProvider.GetRequiredService<ITrafficLogPublisher>();

        // Extract intersection/light from route if available
        var intersection = context.Request.RouteValues.TryGetValue("intersection", out var iVal)
            ? iVal?.ToString() ?? "unknown"
            : "unknown";

        var light = context.Request.RouteValues.TryGetValue("light", out var lVal)
            ? lVal?.ToString() ?? "unknown"
            : "unknown";

        await logPublisher.PublishErrorAsync(
            errorType: errorType,
            message: ex.Message,
            intersection: intersection,
            light: light,
            metadata: new
            {
                Path = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = (int)statusCode,
                Exception = ex.ToString(),
                FallbackApplied = fallback
            });

        // Fallback policy — keep light operational in degraded mode
        if (fallback)
        {
            _logger.LogWarning("[FailoverPolicy] Activated for {Intersection}-{Light}", intersection, light);

            var failoverService = scope.ServiceProvider.GetRequiredService<IFailoverService>();

            // If a specific light is in route values → failover single light
            if (!string.IsNullOrEmpty(light) && light != "unknown")
                await failoverService.ApplyFailoverAsync(intersection, light);
            else
                await failoverService.ApplyFailoverIntersectionAsync(intersection);
        }


        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = errorType,
            details = ex.Message,
            fallbackApplied = fallback
        });
    }
}
