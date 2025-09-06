using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages;

namespace IntersectionControllerStore.Publishers.PriorityPub;

public class PriorityPublisher : IPriorityPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<PriorityPublisher> _logger;
    private readonly string _trafficExchange;
    private readonly string _emergencyKey;
    private readonly string _publicTransportKey;
    private readonly string _cyclistKey;
    private readonly string _pedestrianKey;
    private readonly string _incidentKey;

    private const string ServiceTag = "[" + nameof(PriorityPublisher) + "]";

    public PriorityPublisher(IBus bus, ILogger<PriorityPublisher> logger, IConfiguration configuration)
    {
        _bus = bus;
        _logger = logger;

        _trafficExchange    = configuration["RabbitMQ:Exchanges:Traffic"] ?? "TRAFFIC.EXCHANGE";
        _emergencyKey       = configuration["RabbitMQ:RoutingKeys:Priority:EmergencyVehicle"] ?? "priority.emergency_vehicle.{intersection_id}";
        _publicTransportKey = configuration["RabbitMQ:RoutingKeys:Priority:PublicTransport"] ?? "priority.public_transport.{intersection_id}";
        _cyclistKey         = configuration["RabbitMQ:RoutingKeys:Priority:Cyclist"] ?? "priority.cyclist.{intersection_id}";
        _pedestrianKey      = configuration["RabbitMQ:RoutingKeys:Priority:Pedestrian"] ?? "priority.pedestrian.{intersection_id}";
        _incidentKey        = configuration["RabbitMQ:RoutingKeys:Priority:Incident"] ?? "priority.incident.{intersection_id}";
    }

    public async Task PublishPriorityAsync(Guid intersectionId, string type, Guid? detectionId, string? reason)
    {
        string routingKey = type switch
        {
            "emergency"       => _emergencyKey.Replace("{intersection_id}", intersectionId.ToString()),
            "public_transport"=> _publicTransportKey.Replace("{intersection_id}", intersectionId.ToString()),
            "cyclist"         => _cyclistKey.Replace("{intersection_id}", intersectionId.ToString()),
            "pedestrian"      => _pedestrianKey.Replace("{intersection_id}", intersectionId.ToString()),
            "incident"        => _incidentKey.Replace("{intersection_id}", intersectionId.ToString()),
            _ => throw new ArgumentException($"Unsupported priority type '{type}'", nameof(type))
        };

        var message = new PriorityMessage(intersectionId, type, detectionId, reason, DateTime.UtcNow);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_trafficExchange}"));
        await endpoint.Send(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Priority {Type} published for {IntersectionId} ({RoutingKey})",
            ServiceTag, type, intersectionId, routingKey);
    }
}
