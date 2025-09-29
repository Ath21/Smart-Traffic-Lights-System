using DetectionStore.Domain;
using LogMessages;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DetectionStore.Publishers.Logs;

public class DetectionLogPublisher : IDetectionLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<DetectionLogPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _serviceName = "detection_service";
    private readonly string _auditKey;
    private readonly string _errorKey;

    private const string ServiceTag = "[" + nameof(DetectionLogPublisher) + "]";

    public DetectionLogPublisher(
        IConfiguration config,
        ILogger<DetectionLogPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _auditKey = config["RabbitMQ:RoutingKeys:Log:Audit"] 
                    ?? "log.sensor.detection_service.{intersection}.audit";
        _errorKey = config["RabbitMQ:RoutingKeys:Log:Error"] 
                    ?? "log.sensor.detection_service.{intersection}.error";
    }

    public async Task PublishAuditAsync(string action, string details, object? metadata = null)
    {
        var routingKey = _auditKey.Replace("{intersection}", _intersection.Id.ToString());

        var log = new LogMessage
        {
            Layer = "sensor",
            Service = _serviceName,
            Type = "audit",
            Message = $"{action}: {details}",
            Timestamp = DateTime.UtcNow,
            Metadata = BuildMetadata(metadata)
        };

        await _bus.Publish(log, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "{Tag} AUDIT @ {IntersectionName} (Id={IntersectionId}) -> {Action} | {Details}",
            ServiceTag, _intersection.Name, _intersection.Id, action, details
        );
    }

    public async Task PublishErrorAsync(string errorType, string message, object? metadata = null)
    {
        var routingKey = _errorKey.Replace("{intersection}", _intersection.Id.ToString());

        var log = new LogMessage
        {
            Layer = "sensor",
            Service = _serviceName,
            Type = "error",
            Message = $"{errorType}: {message}",
            Timestamp = DateTime.UtcNow,
            Metadata = BuildMetadata(metadata)
        };

        await _bus.Publish(log, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogError(
            "{Tag} ERROR @ {IntersectionName} (Id={IntersectionId}) -> {Type}: {Message}",
            ServiceTag, _intersection.Name, _intersection.Id, errorType, message
        );
    }

    private Dictionary<string, object>? BuildMetadata(object? metadata)
    {
        var dict = metadata as Dictionary<string, object> ?? new Dictionary<string, object>();
        dict["IntersectionId"] = _intersection.Id;
        dict["IntersectionName"] = _intersection.Name;
        return dict;
    }
}
