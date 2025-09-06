using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace DetectionStore.Publishers.Event;

public class DetectionEventPublisher : IDetectionEventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<DetectionEventPublisher> _logger;
    private readonly string _sensorExchange;
    private readonly string _emergencyKey;
    private readonly string _publicTransportKey;
    private readonly string _incidentKey;

    private const string ServiceTag = "[" + nameof(DetectionEventPublisher) + "]";

    public DetectionEventPublisher(IConfiguration config, ILogger<DetectionEventPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _sensorExchange   = config["RabbitMQ:Exchanges:Sensor"] ?? "SENSOR.EXCHANGE";
        _emergencyKey     = config["RabbitMQ:RoutingKeys:Sensor:EmergencyVehicle"] ?? "sensor.vehicle.emergency.{intersection_id}";
        _publicTransportKey = config["RabbitMQ:RoutingKeys:Sensor:PublicTransport"] ?? "sensor.public_transport.request.{intersection_id}";
        _incidentKey      = config["RabbitMQ:RoutingKeys:Sensor:IncidentDetected"] ?? "sensor.incident.detected.{intersection_id}";
    }

    // sensor.vehicle.emergency.{intersection_id}
    public async Task PublishEmergencyVehicleAsync(Guid intersectionId, bool detected)
    {
        var routingKey = _emergencyKey.Replace("{intersection_id}", intersectionId.ToString());

        var msg = new EmergencyVehicleMessage(Guid.NewGuid(), intersectionId, detected, DateTime.UtcNow);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Emergency vehicle published for {IntersectionId}: {Detected}", 
            ServiceTag, intersectionId, detected);
    }

    // sensor.public_transport.request.{intersection_id}
    public async Task PublishPublicTransportAsync(Guid intersectionId, string? routeId)
    {
        var routingKey = _publicTransportKey.Replace("{intersection_id}", intersectionId.ToString());

        var msg = new PublicTransportMessage(Guid.NewGuid(), intersectionId, routeId, DateTime.UtcNow);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Public transport published for {IntersectionId}, Route {RouteId}", 
            ServiceTag, intersectionId, routeId ?? "N/A");
    }

    // sensor.incident.detected.{intersection_id}
    public async Task PublishIncidentAsync(Guid intersectionId, string description)
    {
        var routingKey = _incidentKey.Replace("{intersection_id}", intersectionId.ToString());

        var msg = new IncidentDetectionMessage(Guid.NewGuid(), intersectionId, description, DateTime.UtcNow);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Incident published for {IntersectionId}: {Description}", 
            ServiceTag, intersectionId, description);
    }
}
