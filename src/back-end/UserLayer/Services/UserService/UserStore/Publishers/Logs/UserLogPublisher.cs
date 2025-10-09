using MassTransit;
using Messages.Log;

namespace UserStore.Publishers.Logs;

public class UserLogPublisher : IUserLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<UserLogPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public UserLogPublisher(
        IConfiguration config,
        ILogger<UserLogPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:User"]
                          ?? "log.user.user.{type}";
    }

    public async Task PublishAuditAsync(string action, string message, Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        var msg = CreateLog("Audit", action, message, metadata, correlationId);
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

        var msg = CreateLog("Error", action, errorMessage, metadata, correlationId);
        await PublishAsync("error", msg);
        _logger.LogError(ex, "ERROR log published (Action={Action}) - {Error}", action, errorMessage);
    }

    public async Task PublishFailoverAsync(string action, string message, Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        var msg = CreateLog("Failover", action, message, metadata, correlationId);
        await PublishAsync("failover", msg);
        _logger.LogWarning("FAILOVER log published (Action={Action})", action);
    }

    private LogMessage CreateLog(string type, string action, string message, Dictionary<string, string>? metadata, Guid? correlationId)
    {
        return new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Layer = "User",
            LogType = type,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "User Service" },
            DestinationServices = new() { "Log Service" },
            Action = action,
            Message = message,
            Metadata = metadata
        };
    }

    private async Task PublishAsync(string logType, LogMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern.Replace("{type}", logType);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
