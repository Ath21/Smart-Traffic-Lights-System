using MassTransit;
using Messages.Log;

namespace NotificationStore.Publishers.Logs;

public class NotificationLogPublisher : INotificationLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<NotificationLogPublisher> _logger;
    private readonly string _routingPattern;

    public NotificationLogPublisher(
        IConfiguration config,
        ILogger<NotificationLogPublisher> logger,
        IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:Notification"]
                          ?? "log.user.notification.{type}";
    }

    public async Task PublishAuditAsync(string action, string message, Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        var msg = CreateBaseLog("Audit", action, message, metadata, correlationId);
        await PublishAsync("audit", msg);
        _logger.LogInformation("AUDIT log published (Action={Action})", action);
    }

    public async Task PublishErrorAsync(string action, string errorMessage, Exception? ex = null, Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        metadata ??= new();
        if (ex != null)
        {
            metadata["exception_type"] = ex.GetType().Name;
            metadata["exception_message"] = ex.Message;
        }

        var msg = CreateBaseLog("Error", action, errorMessage, metadata, correlationId);
        await PublishAsync("error", msg);
        _logger.LogError(ex, "ERROR log published (Action={Action}) - {Error}", action, errorMessage);
    }

    public async Task PublishFailoverAsync(string action, string message, Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        var msg = CreateBaseLog("Failover", action, message, metadata, correlationId);
        await PublishAsync("failover", msg);
        _logger.LogWarning("FAILOVER log published (Action={Action})", action);
    }

    private LogMessage CreateBaseLog(string logType, string action, string message, Dictionary<string, string>? metadata, Guid? correlationId)
    {
        return new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Layer = "User",
            LogType = logType,
            SourceServices = new() { "Notification Service" },
            DestinationServices = new() { "Log Service" },
            Action = action,
            Message = message,
            Metadata = metadata
        };
    }

    private async Task PublishAsync(string type, LogMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern.Replace("{type}", type);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
