using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.Log;
using DetectionStore.Domain;

namespace DetectionStore.Publishers.Logs;

public class DetectionLogPublisher : IDetectionLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<DetectionLogPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public DetectionLogPublisher(
        IConfiguration config,
        ILogger<DetectionLogPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:Detection"]
                          ?? "log.sensor.detection-service.{type}";
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

            SourceService = "Detection Service",
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
