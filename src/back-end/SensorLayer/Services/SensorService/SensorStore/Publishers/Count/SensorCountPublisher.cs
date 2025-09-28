using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace SensorStore.Publishers.Count;

public class SensorCountPublisher : ISensorCountPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<SensorCountPublisher> _logger;
    private readonly string _vehicleKey;
    private readonly string _pedestrianKey;
    private readonly string _cyclistKey;

    private const string ServiceTag = "[" + nameof(SensorCountPublisher) + "]";

    public SensorCountPublisher(IConfiguration config, ILogger<SensorCountPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _vehicleKey    = config["RabbitMQ:RoutingKeys:Sensor:VehicleCount"]    ?? "sensor.count.{intersection}.vehicle";
        _pedestrianKey = config["RabbitMQ:RoutingKeys:Sensor:PedestrianCount"] ?? "sensor.count.{intersection}.pedestrian";
        _cyclistKey    = config["RabbitMQ:RoutingKeys:Sensor:CyclistCount"]    ?? "sensor.count.{intersection}.cyclist";
    }

    public async Task PublishVehicleCountAsync(int intersectionId, int count, float avgSpeed = 0)
    {
        var routingKey = _vehicleKey.Replace("{intersection}", intersectionId.ToString());

        var msg = new SensorCountMessage
        {
            Intersection = intersectionId.ToString(),
            Type = "vehicle",
            Count = count,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Vehicle count published for {IntersectionId}: {Count}", 
            ServiceTag, intersectionId, count);
    }

    public async Task PublishPedestrianCountAsync(int intersectionId, int count)
    {
        var routingKey = _pedestrianKey.Replace("{intersection}", intersectionId.ToString());

        var msg = new SensorCountMessage
        {
            Intersection = intersectionId.ToString(),
            Type = "pedestrian",
            Count = count,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Pedestrian count published for {IntersectionId}: {Count}", 
            ServiceTag, intersectionId, count);
    }

    public async Task PublishCyclistCountAsync(int intersectionId, int count)
    {
        var routingKey = _cyclistKey.Replace("{intersection}", intersectionId.ToString());

        var msg = new SensorCountMessage
        {
            Intersection = intersectionId.ToString(),
            Type = "cyclist",
            Count = count,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Cyclist count published for {IntersectionId}: {Count}", 
            ServiceTag, intersectionId, count);
    }
}
