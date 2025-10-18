using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.Log;

namespace NotificationStore.Publishers.Logs;

public class LogPublisher : ILogPublisher
{
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<LogPublisher> _logger;
    private readonly string _routingKeyPattern;
    private readonly string _source;

    public LogPublisher(IPublishEndpoint publisher, IConfiguration configuration, ILogger<LogPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;

        _routingKeyPattern = configuration["RabbitMQ:RoutingKeys:Log:User"]
            ?? "log.user.notification-api.{type}";
    }

    // -------------------------------------------------------------------
    // Core publisher
    // -------------------------------------------------------------------
    public async Task PublishAsync(
        string type,
        string category,
        string message,
        string? correlationId = null,
        Dictionary<string, object>? data = null)
    {
        try
        {
            var routingKey = _routingKeyPattern.Replace("{type}", type);
            var log = new LogMessage
            {
                Source = _source,
                Type = type,
                Category = category,
                Level = type == "error" ? "error" :
                        type == "failover" ? "critical" : "info",
                Message = message,
                CorrelationId = correlationId,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            await _publisher.Publish(log, ctx =>
            {
                ctx.SetRoutingKey(routingKey);
            });

            _logger.LogInformation("[LOG][PUBLISHER] [{Type}] {Message}", type.ToUpper(), message);
        }
        catch (Exception ex)
        {
            // Local fallback log
            _logger.LogError(ex, "[LOG][PUBLISHER] Failed to publish log: {Message}", message);
        }
    }

    // -------------------------------------------------------------------
    // Helper wrappers
    // -------------------------------------------------------------------
    public Task PublishAuditAsync(string category, string message, string? correlationId = null, Dictionary<string, object>? data = null)
        => PublishAsync("audit", category, message, correlationId, data);

    public Task PublishErrorAsync(string category, string message, string? correlationId = null, Dictionary<string, object>? data = null)
        => PublishAsync("error", category, message, correlationId, data);

    public Task PublishFailoverAsync(string category, string message, string? correlationId = null, Dictionary<string, object>? data = null)
        => PublishAsync("failover", category, message, correlationId, data);
}
