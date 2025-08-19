using System;
using System.Net;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using RabbitMQ.Client.Exceptions;
using TrafficLightCoordinatorStore.Publishers.Logs;
// If you use FluentValidation, uncomment the using below
// using FluentValidation;

namespace TrafficLightCoordinatorStore.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

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
            catch (UnauthorizedAccessException ex)
            {
                await HandleAsync(context, HttpStatusCode.Unauthorized, "Unauthorized access", ex);
            }
            catch (KeyNotFoundException ex)
            {
                await HandleAsync(context, HttpStatusCode.NotFound, "Resource not found", ex);
            }
            // 400 family
            // catch (ValidationException ex) { await HandleAsync(context, HttpStatusCode.BadRequest, "Validation failed", ex); }
            catch (ArgumentNullException ex)
            {
                await HandleAsync(context, HttpStatusCode.BadRequest, "Missing required parameter", ex);
            }
            catch (ArgumentException ex)
            {
                await HandleAsync(context, HttpStatusCode.BadRequest, "Invalid parameter", ex);
            }
            catch (FormatException ex)
            {
                await HandleAsync(context, HttpStatusCode.BadRequest, "Invalid data format", ex);
            }
            catch (InvalidOperationException ex)
            {
                await HandleAsync(context, HttpStatusCode.BadRequest, "Invalid operation", ex);
            }

            // timeouts / cancellations
            catch (TimeoutException ex)
            {
                await HandleAsync(context, HttpStatusCode.RequestTimeout, "Operation timed out", ex);
            }
            catch (RequestTimeoutException ex) // MassTransit request/response timeout
            {
                await HandleAsync(context, HttpStatusCode.RequestTimeout, "Message broker timeout", ex);
            }
            catch (TaskCanceledException ex)
            {
                await HandleAsync(context, HttpStatusCode.RequestTimeout, "Operation canceled or timed out", ex);
            }

            // upstream / infra
            catch (HttpRequestException ex)
            {
                await HandleAsync(context, HttpStatusCode.BadGateway, "Upstream HTTP request failed", ex);
            }
            catch (BrokerUnreachableException ex)
            {
                await HandleAsync(context, HttpStatusCode.BadGateway, "Message broker unreachable", ex);
            }
            catch (ConfigurationException ex) // MassTransit config/bind issues
            {
                await HandleAsync(context, HttpStatusCode.BadGateway, "Message bus configuration error", ex);
            }

            // database
            catch (DbUpdateException ex)
            {
                await HandleAsync(context, HttpStatusCode.Conflict, "Database update failed", ex);
            }
            catch (PostgresException ex)
            {
                await HandleAsync(context, HttpStatusCode.BadGateway, "Database error", ex);
            }

            // last resort
            catch (Exception ex)
            {
                await HandleAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred", ex);
            }
        }

        private async Task HandleAsync(HttpContext ctx, HttpStatusCode status, string message, Exception ex)
        {
            _logger.LogError(ex, "{Message}", message);

            // publish error to bus
            var publisher = ctx.RequestServices.GetService(typeof(ITrafficLogPublisher)) as ITrafficLogPublisher;
            if (publisher != null)
            {
                try { await publisher.PublishErrorAsync(message, ex, ctx.RequestAborted); }
                catch (Exception pubEx)
                {
                    _logger.LogWarning(pubEx, "Failed to publish error log to broker");
                }
            }

            if (!ctx.Response.HasStarted)
            {
                ctx.Response.StatusCode = (int)status;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new
                {
                    error = message,
                    details = ex.Message,
                    traceId = ctx.TraceIdentifier
                });
            }
        }
    }
}
