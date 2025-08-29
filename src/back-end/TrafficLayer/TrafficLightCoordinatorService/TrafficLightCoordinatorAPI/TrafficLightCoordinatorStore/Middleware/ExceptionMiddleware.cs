using System.Net;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RabbitMQ.Client.Exceptions;
using TrafficLightCoordinatorStore.Publishers.Logs;

namespace TrafficLightCoordinatorStore.Middleware
{
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

            //  Authentication / Authorization
            catch (UnauthorizedAccessException ex)
            {
                await HandleAsync(context, HttpStatusCode.Unauthorized, "Unauthorized access", ex);
            }

            //  Not found / bad usage
            catch (KeyNotFoundException ex)
            {
                await HandleAsync(context, HttpStatusCode.NotFound, "Resource not found", ex);
            }
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

            //  Timeouts / Network
            catch (TimeoutException ex)
            {
                await HandleAsync(context, HttpStatusCode.RequestTimeout, "Operation timed out", ex);
            }
            catch (RequestTimeoutException ex) 
            {
                await HandleAsync(context, HttpStatusCode.RequestTimeout, "Message broker timeout", ex);
            }
            catch (TaskCanceledException ex)
            {
                await HandleAsync(context, HttpStatusCode.RequestTimeout, "Operation canceled or timed out", ex);
            }

            //  Upstream / Infra
            catch (HttpRequestException ex)
            {
                await HandleAsync(context, HttpStatusCode.BadGateway, "Upstream HTTP request failed", ex);
            }
            catch (BrokerUnreachableException ex)
            {
                await HandleAsync(context, HttpStatusCode.BadGateway, "Message broker unreachable", ex);
            }
            catch (ConfigurationException ex) // MassTransit configuration error
            {
                await HandleAsync(context, HttpStatusCode.BadGateway, "Message bus configuration error", ex);
            }

            //  Database
            catch (DbUpdateException ex)
            {
                await HandleAsync(context, HttpStatusCode.Conflict, "Database update failed", ex);
            }
            catch (PostgresException ex)
            {
                await HandleAsync(context, HttpStatusCode.BadGateway, "Database error", ex);
            }

            //  Fallback
            catch (Exception ex)
            {
                await HandleAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred", ex);
            }
        }

        private async Task HandleAsync(HttpContext ctx, HttpStatusCode status, string message, Exception ex)
        {
            _logger.LogError(ex, "{Tag} {Message}", ServiceTag, message);

            var publisher = ctx.RequestServices.GetService(typeof(ITrafficLogPublisher)) as ITrafficLogPublisher;
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
                        errorType: ex.GetType().Name,
                        message: message,
                        metadata: metadata,
                        ct: ctx.RequestAborted
                    );
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
                    error = message,
                    details = ex.Message,
                    traceId = ctx.TraceIdentifier
                });
            }
        }
    }
}
