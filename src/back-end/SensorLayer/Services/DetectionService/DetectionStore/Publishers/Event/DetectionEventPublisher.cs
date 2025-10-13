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

        _routingPattern = config["RabbitMQ:RoutingKeys:Detection:Event"]
                          ?? "sensor.detection.{intersection}.{event}";
    }

    public async Task PublishEmergencyVehicleAsync(
        string vehicleType,
        string direction,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var msg = new DetectionEventMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Sensor Layer",
            DestinationLayer = new() { "Traffic Layer" },

            SourceService = "Detection Service",
            DestinationServices = new() { "Intersection Controller Service", "Traffic Analytics Service" },

            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,

            EventType = "Emergency Vehicle",
            VehicleType = vehicleType,
            Direction = direction,

            Metadata = metadata
        };

        await PublishAsync("emergency-vehicle", msg);

        _logger.LogInformation("[PUBLISHER][EVENT][{Intersection}] EMERGENCY VEHICLE published ({VehicleType}, Dir={Direction}",
            _intersection.Name, vehicleType, direction);
    }

    public async Task PublishPublicTransportAsync(
        string vehicleType,
        string direction,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var msg = new DetectionEventMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Sensor Layer",
            DestinationLayer = new() { "Traffic Layer" },

            SourceService = "Detection Service",
            DestinationServices = new() { "Intersection Controller Service", "Traffic Analytics Service" },

            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,

            EventType = "Public Transport",
            VehicleType = vehicleType,
            Direction = direction,

            Metadata = metadata
        };

        await PublishAsync("public-transport", msg);

        _logger.LogInformation("[PUBLISHER][EVENT][{Intersection}] PUBLIC TRANSPORT published ({VehicleType}, Dir={Direction}",
            _intersection.Name, vehicleType, direction);
    }

    public async Task PublishIncidentAsync(
        string vehicleType,
        string direction,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var msg = new DetectionEventMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Sensor Layer",
            DestinationLayer = new() { "Traffic Layer" },

            SourceService = "Detection Service",
            DestinationServices = new() { "Intersection Controller Service", "Traffic Analytics Service" },

            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,

            EventType = "Incident",
            VehicleType = vehicleType,
            Direction = direction,

            Metadata = metadata
        };

        await PublishAsync("incident", msg);

        _logger.LogInformation("[PUBLISHER][EVENT][{Intersection}] INCIDENT published ({VehicleType}, Dir={Direction}",
            _intersection.Name, vehicleType, direction);
    }
    private async Task PublishAsync(string eventKey, DetectionEventMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern
            .Replace("{intersection}", _intersection.Name.ToLower().Replace(' ', '-'))
            .Replace("{event}", eventKey);

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
