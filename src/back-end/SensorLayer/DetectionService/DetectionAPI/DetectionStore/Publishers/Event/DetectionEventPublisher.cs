using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace DetectionStore.Publishers;

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
        _emergencyKey     = config["RabbitMQ:RoutingKeys:Sensor:EmergencyVehicle"] 
                            ?? "sensor.emergency_vehicle.detected.{intersection_id}";
        _publicTransportKey = config["RabbitMQ:RoutingKeys:Sensor:PublicTransport"] 
                            ?? "sensor.public_transport.detected.{intersection_id}";
        _incidentKey      = config["RabbitMQ:RoutingKeys:Sensor:IncidentDetected"] 
                            ?? "sensor.incident.detected.{intersection_id}";
    }

    // sensor.emergency_vehicle.detected.{intersection_id}
    public async Task PublishEmergencyVehicleAsync(Guid intersectionId, DateTime detectedAt)
    {
        var routingKey = _emergencyKey.Replace("{intersection_id}", intersectionId.ToString());
        var msg = new EmergencyVehicleMessage(intersectionId, detectedAt);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Emergency vehicle detected at {IntersectionId} ({RoutingKey})",
            ServiceTag, intersectionId, routingKey);
    }

    // sensor.public_transport.detected.{intersection_id}
    public async Task PublishPublicTransportAsync(Guid intersectionId, DateTime detectedAt, string lineId)
    {
        var routingKey = _publicTransportKey.Replace("{intersection_id}", intersectionId.ToString());
        var msg = new PublicTransportMessage(intersectionId, lineId, detectedAt);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Public transport detected at {IntersectionId} (Line {LineId}) ({RoutingKey})",
            ServiceTag, intersectionId, lineId, routingKey);
    }

    // sensor.incident.detected.{intersection_id}
    public async Task PublishIncidentAsync(Guid intersectionId, string description, DateTime detectedAt)
    {
        var routingKey = _incidentKey.Replace("{intersection_id}", intersectionId.ToString());
        var msg = new IncidentDetectionMessage(intersectionId, description, detectedAt);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_sensorExchange}"));
        await endpoint.Send(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogWarning("{Tag} Incident detected at {IntersectionId}: {Desc} ({RoutingKey})",
            ServiceTag, intersectionId, description, routingKey);
    }
}
