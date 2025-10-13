using MassTransit;
using Messages.Log;
using SensorStore.Domain;

namespace SensorStore.Publishers.Logs;

public class SensorLogPublisher : ISensorLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<SensorLogPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public SensorLogPublisher(
        IConfiguration config,
        ILogger<SensorLogPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:Sensor"]
                          ?? "log.sensor.sensor-service.{type}";
    }

    // ===========================
    // AUDIT
    // ===========================
    public async Task PublishAuditAsync(string action, string message,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        var log = new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Layer = "Sensor",
            LogType = "Audit",
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "Sensor Service" },
            DestinationServices = new() { "Log Service" },
            Action = action,
            Message = message,
            Metadata = metadata
        };

        await PublishAsync("audit", log);
        _logger.LogInformation("[{Intersection}] AUDIT log published (Action={Action})", _intersection.Name, action);
    }

    // ===========================
    // ERROR
    // ===========================
    public async Task PublishErrorAsync(string action, string errorMessage,
        Exception? ex = null, Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        metadata ??= new();
        if (ex != null)
        {
            metadata["exception_type"] = ex.GetType().Name;
            metadata["exception_message"] = ex.Message;
        }

        var log = new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Layer = "Sensor",
            LogType = "Error",
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "Sensor Service" },
            DestinationServices = new() { "Log Service" },
            Action = action,
            Message = errorMessage,
            Metadata = metadata
        };

        await PublishAsync("error", log);
        _logger.LogError(ex, "[{Intersection}] ERROR log published (Action={Action}) - {ErrorMessage}",
            _intersection.Name, action, errorMessage);
    }

    // ===========================
    // FAILOVER
    // ===========================
    public async Task PublishFailoverAsync(string action, string message,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        var log = new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Layer = "Sensor",
            LogType = "Failover",
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "Sensor Service" },
            DestinationServices = new() { "Log Service" },
            Action = action,
            Message = message,
            Metadata = metadata
        };

        await PublishAsync("failover", log);
        _logger.LogWarning("[{Intersection}] FAILOVER log published (Action={Action})", _intersection.Name, action);
    }

    // ===========================
    // Internal publish logic
    // ===========================
    private async Task PublishAsync(string logType, LogMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern.Replace("{type}", logType);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
