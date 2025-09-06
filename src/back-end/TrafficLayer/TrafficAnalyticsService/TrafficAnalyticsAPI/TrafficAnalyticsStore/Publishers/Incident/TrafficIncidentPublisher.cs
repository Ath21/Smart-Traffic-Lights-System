using MassTransit;
using TrafficMessages;

namespace TrafficAnalyticsStore.Publishers.Incident;

public class TrafficIncidentPublisher : ITrafficIncidentPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficIncidentPublisher> _logger;
    private readonly string _incidentKey;

    private const string ServiceTag = "[" + nameof(TrafficIncidentPublisher) + "]";

    public TrafficIncidentPublisher(IConfiguration configuration, ILogger<TrafficIncidentPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _incidentKey = configuration["RabbitMQ:RoutingKeys:Traffic:Incident"] 
                       ?? "traffic.analytics.incident.{intersection_id}";
    }

    public async Task PublishIncidentAsync(TrafficIncidentMessage message)
    {
        var routingKey = _incidentKey.Replace("{intersection_id}", message.IntersectionId.ToString());

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "{Tag} Published Traffic Incident {IncidentId} for Intersection {IntersectionId}: {Description}",
            ServiceTag, message.IncidentId, message.IntersectionId, message.Description
        );
    }
}
