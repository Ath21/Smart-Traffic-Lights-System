using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DetectionStore.Domain;
using SensorMessages.SensorEvent;

namespace DetectionStore.Publishers.Event;

public class DetectionEventPublisher : IDetectionEventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<DetectionEventPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _detectionKey;

    public DetectionEventPublisher(
        IConfiguration config,
        ILogger<DetectionEventPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _detectionKey = config["RabbitMQ:RoutingKeys:Sensor:Detection"]
                        ?? "sensor.detection.{intersection}.{event}";
    }

    public async Task PublishEmergencyVehicleAsync(string type, int priority, string direction)
    {
        var routingKey = _detectionKey
            .Replace("{intersection}", _intersection.Id.ToString())
            .Replace("{event}", "emergency_vehicle");

        var msg = new EmergencyVehicleDetectedMessage
        {
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Type = type,
            PriorityLevel = priority,
            Direction = direction,
            Detected = true,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[{IntersectionName}][ID={IntersectionId}] Emergency vehicle published: {Type} (Priority={PriorityLevel}, Dir={Direction})",
            _intersection.Name, _intersection.Id, type, priority, direction);
    }

    public async Task PublishPublicTransportAsync(string mode, string direction)
    {
        var routingKey = _detectionKey
            .Replace("{intersection}", _intersection.Id.ToString())
            .Replace("{event}", "public_transport");

        var msg = new PublicTransportDetectedMessage
        {
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Mode = mode,
            Direction = direction,
            Detected = true,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[{IntersectionName}][ID={IntersectionId}] Public transport published: {Mode}, Dir={Direction}",
            _intersection.Name, _intersection.Id, mode, direction);
    }

    public async Task PublishIncidentAsync(string type, int severity, string description, string direction)
    {
        var routingKey = _detectionKey
            .Replace("{intersection}", _intersection.Id.ToString())
            .Replace("{event}", "incident");

        var msg = new IncidentDetectedMessage
        {
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Type = type,
            Severity = severity,
            Description = description,
            Direction = direction,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogWarning("[{IntersectionName}][ID={IntersectionId}] Incident published: {Type} (Severity={Severity}, Dir={Direction}) - {Description}",
            _intersection.Name, _intersection.Id, type, severity, direction, description);
    }
}
