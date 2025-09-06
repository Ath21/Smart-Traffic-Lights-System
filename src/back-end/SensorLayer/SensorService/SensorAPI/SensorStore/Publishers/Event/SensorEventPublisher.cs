using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace SensorStore.Publishers;

public class SensorEventPublisher : ISensorEventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<SensorEventPublisher> _logger;
    private readonly string _sensorExchange;
    private readonly string _vehicleKey;
    private readonly string _pedestrianKey;
    private readonly string _cyclistKey;

    private const string ServiceTag = "[" + nameof(SensorEventPublisher) + "]";

    public SensorEventPublisher(IConfiguration config, ILogger<SensorEventPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _sensorExchange = config["RabbitMQ:Exchanges:Sensor"] ?? "SENSOR.EXCHANGE";
        _vehicleKey     = config["RabbitMQ:RoutingKeys:Sensor:VehicleCount"] 
                          ?? "sensor.vehicle.count.{intersection_id}";
        _pedestrianKey  = config["RabbitMQ:RoutingKeys:Sensor:PedestrianCount"] 
                          ?? "sensor.pedestrian.count.{intersection_id}";
        _cyclistKey     = config["RabbitMQ:RoutingKeys:Sensor:CyclistCount"] 
                          ?? "sensor.cyclist.count.{intersection_id}";
    }

    // sensor.vehicle.count.{intersection_id}
    public async Task PublishVehicleCountAsync(Guid intersectionId, int count, DateTime detectedAt)
    {
        var routingKey = _vehicleKey.Replace("{intersection_id}", intersectionId.ToString());
        var msg = new VehicleCountMessage(intersectionId, count, detectedAt);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Vehicle count published for {IntersectionId}: {Count} ({RoutingKey})",
            ServiceTag, intersectionId, count, routingKey);
    }

    // sensor.pedestrian.count.{intersection_id}
    public async Task PublishPedestrianCountAsync(Guid intersectionId, int count, DateTime detectedAt)
    {
        var routingKey = _pedestrianKey.Replace("{intersection_id}", intersectionId.ToString());
        var msg = new PedestrianDetectionMessage(intersectionId, count, detectedAt);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Pedestrian count published for {IntersectionId}: {Count} ({RoutingKey})",
            ServiceTag, intersectionId, count, routingKey);
    }

    // sensor.cyclist.count.{intersection_id}
    public async Task PublishCyclistCountAsync(Guid intersectionId, int count, DateTime detectedAt)
    {
        var routingKey = _cyclistKey.Replace("{intersection_id}", intersectionId.ToString());
        var msg = new CyclistDetectionMessage(intersectionId, count, detectedAt);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Cyclist count published for {IntersectionId}: {Count} ({RoutingKey})",
            ServiceTag, intersectionId, count, routingKey);
    }
}
