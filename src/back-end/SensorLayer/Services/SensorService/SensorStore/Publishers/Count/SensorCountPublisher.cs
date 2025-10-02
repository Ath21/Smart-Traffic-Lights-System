using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages.SensorCount;
using SensorStore.Domain;

namespace SensorStore.Publishers.Count;

public class SensorCountPublisher : ISensorCountPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<SensorCountPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _vehicleKey;
    private readonly string _pedestrianKey;
    private readonly string _cyclistKey;

    public SensorCountPublisher(
        IConfiguration config,
        ILogger<SensorCountPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _vehicleKey = config["RabbitMQ:RoutingKeys:Sensor:VehicleCount"] ?? "sensor.count.{intersection}.vehicle";
        _pedestrianKey = config["RabbitMQ:RoutingKeys:Sensor:PedestrianCount"] ?? "sensor.count.{intersection}.pedestrian";
        _cyclistKey = config["RabbitMQ:RoutingKeys:Sensor:CyclistCount"] ?? "sensor.count.{intersection}.cyclist";
    }

    public async Task PublishVehicleCountAsync(int count, double avgSpeed, string direction)
    {
        var routingKey = _vehicleKey.Replace("{intersection}", _intersection.Id.ToString());

        var msg = new VehicleCountMessage
        {
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Count = count,
            AvgSpeed = avgSpeed,
            Direction = direction,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[{IntersectionName}][ID={IntersectionId}] Vehicle count published: {Count} (AvgSpeed={AvgSpeed}, Dir={Direction})",
            _intersection.Name, _intersection.Id, count, avgSpeed, direction);
    }

    public async Task PublishPedestrianCountAsync(int count, string direction)
    {
        var routingKey = _pedestrianKey.Replace("{intersection}", _intersection.Id.ToString());

        var msg = new PedestrianCountMessage
        {
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Count = count,
            Direction = direction,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[{IntersectionName}][ID={IntersectionId}] Pedestrian count published: {Count} (Dir={Direction})",
            _intersection.Name, _intersection.Id, count, direction);
    }

    public async Task PublishCyclistCountAsync(int count, string direction)
    {
        var routingKey = _cyclistKey.Replace("{intersection}", _intersection.Id.ToString());

        var msg = new CyclistCountMessage
        {
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            Count = count,
            Direction = direction,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[{IntersectionName}][ID={IntersectionId}] Cyclist count published: {Count} (Dir={Direction})",
            _intersection.Name, _intersection.Id, count, direction);
    }
}
