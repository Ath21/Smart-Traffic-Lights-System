using MassTransit;
using TrafficMessages;

namespace TrafficAnalyticsStore.Publishers.Congestion;

public class TrafficCongestionPublisher : ITrafficCongestionPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficCongestionPublisher> _logger;
    private readonly string _congestionKey;

    private const string ServiceTag = "[" + nameof(TrafficCongestionPublisher) + "]";

    public TrafficCongestionPublisher(IConfiguration configuration, ILogger<TrafficCongestionPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _congestionKey = configuration["RabbitMQ:RoutingKeys:TrafficCongestion"] 
                         ?? "traffic.analytics.congestion.{intersection_id}";
    }

    // traffic.analytics.congestion.{intersection_id}
    public async Task PublishCongestionAsync(TrafficCongestionMessage message)
    {
        var routingKey = _congestionKey.Replace("{intersection_id}", message.IntersectionId.ToString());

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "{Tag} Published Traffic Congestion Alert {AlertId} for Intersection {IntersectionId} with level {Level}",
            ServiceTag, message.AlertId, message.IntersectionId, message.CongestionLevel
        );
    }
}
