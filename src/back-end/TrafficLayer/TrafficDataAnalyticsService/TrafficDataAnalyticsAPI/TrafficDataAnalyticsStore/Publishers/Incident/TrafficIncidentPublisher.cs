using System;
using MassTransit;
using TrafficMessages;

namespace TrafficDataAnalyticsStore.Publishers.Incident;

public class TrafficIncidentPublisher : ITrafficIncidentPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficIncidentPublisher> _logger;
    private readonly string _incidentKey;

    public TrafficIncidentPublisher(IConfiguration configuration, ILogger<TrafficIncidentPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;
        _incidentKey = configuration["RabbitMQ:RoutingKeys:TrafficIncident"] ?? "traffic.analytics.incident";
    }

    public async Task PublishIncidentAsync(TrafficIncidentMessage message)
    {
        var routingKey = $"{_incidentKey}.{message.IntersectionId}";
        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "Published Traffic Incident {IncidentId} for Intersection {IntersectionId}: {Description}",
            message.IncidentId, message.IntersectionId, message.Description
        );
    }
}
