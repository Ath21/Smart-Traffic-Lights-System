using MassTransit;
using Messages.Log;

namespace NotificationStore.Publishers.Logs;

public class NotificationLogPublisher : INotificationLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<NotificationLogPublisher> _logger;
    private readonly string _routingPattern;

    public NotificationLogPublisher(IBus bus, IConfiguration config, ILogger<NotificationLogPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        // Pattern: log.user.notification.{type}
        _routingPattern = config["RabbitMQ:RoutingKeys:Log:Notification"]
                          ?? "log.user.notification-service.{type}";
    }

    public async Task PublishAuditAsync(string action, string message, Dictionary<string, string>? metadata = null)
    {
        var msg = new LogMessage
        {
            CorrelationId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "User Layer",
            DestinationLayer = new() { "Log Layer" },

            SourceService = "Notification Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Audit",
            Action = action,
            Message = message,
            Metadata = metadata
        };

        var routingKey = _routingPattern.Replace("{type}", "audit");

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[PUBLISHER][LOG][AUDIT] Action={Action}, Msg={Message}", action, message);
    }

    public async Task PublishErrorAsync(string action, string message, Dictionary<string, string>? metadata = null)
    {
        var msg = new LogMessage
        {
            CorrelationId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "User Layer",
            DestinationLayer = new() { "Log Layer" },

            SourceService = "Notification Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Error",
            Action = action,
            Message = message,
            Metadata = metadata
        };

        var routingKey = _routingPattern.Replace("{type}", "error");

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogWarning("[PUBLISHER][LOG][ERROR] Action={Action}, Msg={Message}", action, message);
    }
}