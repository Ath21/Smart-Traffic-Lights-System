using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DetectionStore.Domain;
using Messages.Sensor;

namespace DetectionStore.Publishers.Event;

public class DetectionEventPublisher : IDetectionEventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<DetectionEventPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public DetectionEventPublisher(
        IConfiguration config,
        ILogger<DetectionEventPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Sensor:Detection"]
                          ?? "sensor.detection.{intersection}.{event}";
    }

    public async Task PublishEmergencyVehicleAsync(
        string vehicleType,
        int speed_kmh,
        string direction,
        Guid? correlationId = null)
    {
        await PublishAsync("emergency_vehicle", new DetectionEventMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            EventType = "EmergencyVehicle",
            VehicleType = vehicleType,
            Direction = direction,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Metadata = new()
            {
                ["SpeedKmh"] = speed_kmh.ToString()
            }
        });

        _logger.LogInformation("[{Intersection}] Emergency vehicle published ({VehicleType}, Dir={Direction}, Speed={SpeedKmh})",
            _intersection.Name, vehicleType, direction, speed_kmh);
    }

    public async Task PublishPublicTransportAsync(
        string mode,
        string line,
        int arrival_estimated_sec,
        string direction,
        Guid? correlationId = null)
    {
        await PublishAsync("public_transport", new DetectionEventMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            EventType = "PublicTransport",
            VehicleType = mode,
            Direction = direction,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Metadata = new()
            {
                ["Line"] = line,
                ["ArrivalEstimatedSec"] = arrival_estimated_sec.ToString()
            }
        });

        _logger.LogInformation("[{Intersection}] Public transport published ({Mode}, Line={Line}, ArrivalInSec={ArrivalInSec}, Dir={Direction})",
            _intersection.Name, mode, line, arrival_estimated_sec, direction);
    }

    public async Task PublishIncidentAsync(
        string type,
        int severity,
        string description,
        string direction,
        Guid? correlationId = null)
    {
        await PublishAsync("incident", new DetectionEventMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            EventType = type,
            Direction = direction,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Metadata = new()
            {
                ["Severity"] = severity.ToString(),
                ["Description"] = description
            }
        });

        _logger.LogWarning("[{Intersection}] Incident published: {Type} (Severity={Severity}, Dir={Direction}) - {Description}",
            _intersection.Name, type, severity, direction, description);
    }

    private async Task PublishAsync(string eventKey, DetectionEventMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern
            .Replace("{intersection}", _intersection.Name.ToLower().Replace(' ', '-'))
            .Replace("{event}", eventKey);

        msg.SourceServices = new() { "Detection Service" };
        msg.DestinationServices = new() { "Intersection Controller Service" };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
