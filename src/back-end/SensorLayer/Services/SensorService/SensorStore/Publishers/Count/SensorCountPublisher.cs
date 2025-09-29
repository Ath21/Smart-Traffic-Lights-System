using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;
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

    private const string ServiceTag = "[" + nameof(SensorCountPublisher) + "]";

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

    public async Task PublishVehicleCountAsync(int count, float avgSpeed = 0)
    {
        var routingKey = _vehicleKey.Replace("{intersection}", _intersection.Id.ToString());

        var msg = new SensorCountMessage
        {
            Intersection = _intersection.Id.ToString(),
            Type = "vehicle",
            Count = count,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Vehicle count published for {IntersectionName} (Id={IntersectionId}): {Count}",
            ServiceTag, _intersection.Name, _intersection.Id, count);
    }

    public async Task PublishPedestrianCountAsync(int count)
    {
        var routingKey = _pedestrianKey.Replace("{intersection}", _intersection.Id.ToString());

        var msg = new SensorCountMessage
        {
            Intersection = _intersection.Id.ToString(),
            Type = "pedestrian",
            Count = count,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Pedestrian count published for {IntersectionName} (Id={IntersectionId}): {Count}",
            ServiceTag, _intersection.Name, _intersection.Id, count);
    }

    public async Task PublishCyclistCountAsync(int count)
    {
        var routingKey = _cyclistKey.Replace("{intersection}", _intersection.Id.ToString());

        var msg = new SensorCountMessage
        {
            Intersection = _intersection.Id.ToString(),
            Type = "cyclist",
            Count = count,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Cyclist count published for {IntersectionName} (Id={IntersectionId}): {Count}",
            ServiceTag, _intersection.Name, _intersection.Id, count);
    }
}
