using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace SensorStore.Publishers.Count;

public class SensorCountPublisher : ISensorCountPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<SensorCountPublisher> _logger;
    private readonly string _sensorExchange;
    private readonly string _vehicleKey;
    private readonly string _pedestrianKey;
    private readonly string _cyclistKey;

    private const string ServiceTag = "[" + nameof(SensorCountPublisher) + "]";

    public SensorCountPublisher(IConfiguration config, ILogger<SensorCountPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _sensorExchange = config["RabbitMQ:Exchanges:Sensor"] ?? "SENSOR.EXCHANGE";
        _vehicleKey     = config["RabbitMQ:RoutingKeys:Sensor:VehicleCount"] ?? "sensor.vehicle.count.{intersection_id}";
        _pedestrianKey  = config["RabbitMQ:RoutingKeys:Sensor:PedestrianCount"] ?? "sensor.pedestrian.request.{intersection_id}";
        _cyclistKey     = config["RabbitMQ:RoutingKeys:Sensor:CyclistCount"] ?? "sensor.cyclist.request.{intersection_id}";
    }

    // sensor.vehicle.count.{intersection_id}
    public async Task PublishVehicleCountAsync(Guid intersectionId, int count, float avgSpeed)
    {
        var routingKey = _vehicleKey.Replace("{intersection_id}", intersectionId.ToString());

        var msg = new VehicleCountMessage(Guid.NewGuid(), intersectionId, count, avgSpeed, DateTime.UtcNow);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Vehicle count published for {IntersectionId}: {Count}, Speed {Speed}", 
            ServiceTag, intersectionId, count, avgSpeed);
    }

    // sensor.pedestrian.request.{intersection_id}
    public async Task PublishPedestrianCountAsync(Guid intersectionId, int count)
    {
        var routingKey = _pedestrianKey.Replace("{intersection_id}", intersectionId.ToString());

        var msg = new PedestrianDetectionMessage(Guid.NewGuid(), intersectionId, count, DateTime.UtcNow);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Pedestrian count published for {IntersectionId}: {Count}", 
            ServiceTag, intersectionId, count);
    }

    // sensor.cyclist.request.{intersection_id}
    public async Task PublishCyclistCountAsync(Guid intersectionId, int count)
    {
        var routingKey = _cyclistKey.Replace("{intersection_id}", intersectionId.ToString());

        var msg = new CyclistDetectionMessage(Guid.NewGuid(), intersectionId, count, DateTime.UtcNow);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Cyclist count published for {IntersectionId}: {Count}", 
            ServiceTag, intersectionId, count);
    }
}
