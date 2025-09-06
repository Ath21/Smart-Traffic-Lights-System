using System;
using System.Net;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TrafficLightControllerStore.Publishers.Logs;

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
        catch (UnauthorizedAccessException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.Unauthorized, "UnauthorizedAccess", ex);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.NotFound, "ResourceNotFound", ex);
        }
        catch (InvalidOperationException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "InvalidOperation", ex);
        }
        catch (ArgumentNullException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "ArgumentNull", ex);
        }
        catch (ArgumentException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Argument", ex);
        }
        catch (FormatException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Format", ex);
        }
        catch (System.Text.Json.JsonException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "JsonParse", ex);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadRequest, "Validation", ex);
        }
        catch (TimeoutException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "Timeout", ex);
        }
        catch (HttpRequestException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.BadGateway, "HttpRequest", ex);
        }
        catch (TaskCanceledException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.RequestTimeout, "TaskCanceled", ex);
        }
        catch (NotSupportedException ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.NotImplemented, "NotSupported", ex);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(context, HttpStatusCode.InternalServerError, "Unhandled", ex);
        }
    }

    private async Task HandleErrorAsync(HttpContext context, HttpStatusCode statusCode, string errorType, Exception ex)
    {
        _logger.LogError(ex, "[{Type}] {Message}", errorType, ex.Message);

        using var scope = _scopeFactory.CreateScope();
        var logPublisher = scope.ServiceProvider.GetRequiredService<ITrafficLogPublisher>();

        // 🔹 Publish structured error log
        await logPublisher.PublishErrorAsync(
            errorType: errorType,
            message: ex.Message,
            metadata: new
            {
                Path = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = (int)statusCode,
                Exception = ex.ToString()
            });

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = errorType,
            details = ex.Message
        });
    }
}
