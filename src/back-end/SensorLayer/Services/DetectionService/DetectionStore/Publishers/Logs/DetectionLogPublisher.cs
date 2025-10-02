using LogMessages;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DetectionStore.Domain;

namespace DetectionStore.Publishers.Logs;

public class DetectionLogPublisher : IDetectionLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<DetectionLogPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _auditKey;
    private readonly string _errorKey;

    private const string Layer = "Sensor";
    private const string ServiceName = "Detection";

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
            Layer = Layer,
            Service = ServiceName,
            Type = "audit",
            Message = $"{action}: {details}",
            Timestamp = DateTime.UtcNow,
            Metadata = BuildMetadata(metadata)
        };

        await _bus.Publish(log, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[{IntersectionName}][ID={IntersectionId}][AUDIT] {Action} -> {Details}",
            _intersection.Name, _intersection.Id, action, details);
    }

    public async Task PublishErrorAsync(string errorType, string message, object? metadata = null)
    {
        var routingKey = _errorKey.Replace("{intersection}", _intersection.Id.ToString());

        var log = new LogMessage
        {
            Layer = Layer,
            Service = ServiceName,
            Type = "error",
            Message = $"{errorType}: {message}",
            Timestamp = DateTime.UtcNow,
            Metadata = BuildMetadata(metadata)
        };

        await _bus.Publish(log, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogError("[{IntersectionName}][ID={IntersectionId}][ERROR] {Type} -> {Message}",
            _intersection.Name, _intersection.Id, errorType, message);
    }

    private Dictionary<string, object>? BuildMetadata(object? metadata)
    {
        var dict = metadata as Dictionary<string, object> ?? new Dictionary<string, object>();
        dict["IntersectionId"] = _intersection.Id;
        dict["IntersectionName"] = _intersection.Name;
        return dict;
    }
}
