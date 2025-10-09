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
                          ?? "log.sensor.detection.{type}";
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
            Layer = "Sensor",
            LogType = "Audit",
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "Detection Service" },
            DestinationServices = new() { "Log Service" },
            Action = action,
            Message = message,
            Metadata = metadata
        });

        _logger.LogInformation("[{Intersection}] AUDIT log published (Action={Action})", _intersection.Name, action);
    }

    public async Task PublishErrorAsync(
        string action,
        string errorMessage,
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
            Layer = "Sensor",
            LogType = "Error",
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "Detection Service" },
            DestinationServices = new() { "Log Service" },
            Action = action,
            Message = errorMessage,
            Metadata = metadata
        });

        _logger.LogError(ex, "[{Intersection}] ERROR log published (Action={Action}) - {ErrorMessage}",
            _intersection.Name, action, errorMessage);
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
            Layer = "Sensor",
            LogType = "Failover",
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "Detection Service" },
            DestinationServices = new() { "Log Service" },
            Action = action,
            Message = message,
            Metadata = metadata
        });

        _logger.LogWarning("[{Intersection}] FAILOVER log published (Action={Action})", _intersection.Name, action);
    }

    private async Task PublishAsync(string logType, LogMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern.Replace("{type}", logType);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
