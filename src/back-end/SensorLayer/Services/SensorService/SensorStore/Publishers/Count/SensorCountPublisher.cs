using MassTransit;
using SensorStore.Domain;

namespace SensorStore.Publishers.Count;

public class SensorCountPublisher : ISensorCountPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<SensorCountPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public SensorCountPublisherq(
        IConfiguration config,
        ILogger<SensorCountPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Sensor:Count"]
                          ?? "sensor.count.{intersection}.{event}";
    }

    public async Task PublishVehicleAsync(string vehicleType, int speed_kmh, string direction)
    {
        await PublishAsync("vehicle", new SensorCountMessage
        {
            EventType = "EmergencyVehicle",
            VehicleType = vehicleType,
            Direction = direction,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Timestamp = DateTime.UtcNow,
            Metadata = new()
            {
                ["SpeedKmh"] = speed_kmh.ToString()
            }
        });

        _logger.LogInformation("[{IntersectionName}] Emergency vehicle published ({VehicleType}, Dir={Direction}, Speed={SpeedKmh})",
            _intersection.Name, vehicleType, direction, speed_kmh);
    }

    public async Task PublishPedestrianAsync(string mode, string line, int arrival_estimated_sec, string direction)
    {
        await PublishAsync("pedestrian", new SensorCountMessage
        {
            EventType = "Pedestrian",
            VehicleType = mode,
            Direction = direction,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Timestamp = DateTime.UtcNow,
            Metadata = new()
            {
                ["Line"] = line,
                ["ArrivalEstimatedSec"] = arrival_estimated_sec.ToString()
            }
        });

        _logger.LogInformation("[{IntersectionName}] Public transport published ({Mode}, Line={Line}, ArrivalInSec={ArrivalEstimatedSec}, Dir={Direction})",
            _intersection.Name, mode, line, arrival_estimated_sec, direction);
    }

    public async Task PublishCyclistAsync(string type, int severity, string description, string direction)
    {
        await PublishAsync("cyclist", new SensorCountMessage
        {
            EventType = type,
            Direction = direction,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Timestamp = DateTime.UtcNow,
            Metadata = new()
            {
                ["Severity"] = severity.ToString(),
                ["Description"] = description
            }
        });

        _logger.LogWarning("[{IntersectionName}] Incident published: {Type} (Severity={Severity}, Dir={Direction}) - {Description}",
            _intersection.Name, type, severity, direction, description);
    }

    private async Task PublishAsync(string eventKey, SensorCountMessage msg)
    {
        var routingKey = _routingPattern
            .Replace("{intersection}", _intersection.Name.ToLower().Replace(' ', '-'))
            .Replace("{count}", eventKey);

        msg.CorrelationId = Guid.NewGuid();

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
