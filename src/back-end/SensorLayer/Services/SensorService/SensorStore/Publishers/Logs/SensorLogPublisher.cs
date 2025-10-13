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
        IBus bus,
        IConfiguration config,
        ILogger<SensorLogPublisher> logger,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:Sensor"]
                          ?? "log.sensor.sensor-service.{type}";
    }

    public async Task PublishAuditAsync(
        string action,
        string message,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        await PublishAsync("audit", new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Sensor Layer",
            DestinationLayer = new () { "Log Layer" },

            SourceService = "Detection Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Audit",

            Action = action,
            Message = message,

            Metadata = metadata
        });

        _logger.LogInformation("[PUBLISHER][LOG][{Intersection}] AUDIT log published (Action={Action}) - Message={Message}", _intersection.Name, action, message);
    }

    public async Task PublishErrorAsync(
        string action,
        string message,
        Exception? ex = null,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        metadata ??= new();
        if (ex != null)
        {
            metadata["exception_type"] = ex.GetType().Name;
            metadata["exception_message"] = ex.Message;
        }

        await PublishAsync("error", new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Sensor Layer",
            DestinationLayer = new () { "Log Layer" },

            SourceService = "Sensor Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Error",

            Action = action,
            Message = message,

            Metadata = metadata
        });

        _logger.LogError("[PUBLISHER][LOG][{Intersection}] ERROR log published (Action={Action}) - Message={Message}", _intersection.Name, action, message);
    }

    public async Task PublishFailoverAsync(
        string action,
        string message,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        await PublishAsync("failover", new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Sensor Layer",
            DestinationLayer = new () { "Log Layer" },

            SourceService = "Detection Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Failover",

            Action = action,
            Message = message,

            Metadata = metadata
        });

        _logger.LogWarning("[PUBLISHER][LOG][{Intersection}] FAILOVER log published (Action={Action} - Message={Message})", _intersection.Name, action, message);
    }

    private async Task PublishAsync(string logType, LogMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern.Replace("{type}", logType);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
