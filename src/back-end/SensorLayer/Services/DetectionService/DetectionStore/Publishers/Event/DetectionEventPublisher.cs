using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.Sensor.Detection;
using DetectionStore.Domain; 

namespace DetectionStore.Publishers.Event;

public class DetectionEventPublisher : IDetectionEventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<DetectionEventPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public DetectionEventPublisher(
        IBus bus,
        IConfiguration config,
        ILogger<DetectionEventPublisher> logger,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;
        _routingPattern = config["RabbitMQ:RoutingKeys:Sensor:DetectionEvent"]
                          ?? "sensor.detection.{intersection}.{event}";
    }

    public async Task PublishEmergencyVehicleDetectedAsync(EmergencyVehicleDetectedMessage message)
    {
        var routingKey = BuildRoutingKey("emergency");
        message.CorrelationId ??= Guid.NewGuid().ToString();
        message.DetectedAt = DateTime.UtcNow;
        message.IntersectionId = _intersection.Id;
        message.Intersection = _intersection.Name;

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));
        _logger.LogInformation("[PUBLISHER][DETECTION] Emergency vehicle detected at {Intersection} ({Type})",
            _intersection.Name, message.EmergencyVehicleType);
    }

    public async Task PublishPublicTransportDetectedAsync(PublicTransportDetectedMessage message)
    {
        var routingKey = BuildRoutingKey("public-transport");
        message.CorrelationId ??= Guid.NewGuid().ToString();
        message.DetectedAt = DateTime.UtcNow;
        message.IntersectionId = _intersection.Id;
        message.IntersectionName = _intersection.Name;

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));
        _logger.LogInformation("[PUBLISHER][DETECTION] Public transport detected at {Intersection} (Line={Line})",
            _intersection.Name, message.LineName);
    }

    public async Task PublishIncidentDetectedAsync(IncidentDetectedMessage message)
    {
        var routingKey = BuildRoutingKey("incident");
        message.CorrelationId ??= Guid.NewGuid().ToString();
        message.ReportedAt = DateTime.UtcNow;
        message.IntersectionId = _intersection.Id;
        message.Intersection = _intersection.Name;

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));
        _logger.LogInformation("[PUBLISHER][DETECTION] Incident detected at {Intersection}: {Description}",
            _intersection.Name, message.Description);
    }

    private string BuildRoutingKey(string eventType)
        => _routingPattern.Replace("{intersection}", _intersection.Name.ToLower().Replace(' ', '-'))
                          .Replace("{event}", eventType);
}
