using System;
using MassTransit;
using TrafficMessages;

namespace TrafficDataAnalyticsStore.Publishers.Congestion;

public class TrafficCongestionPublisher : ITrafficCongestionPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficCongestionPublisher> _logger;
    private readonly string _congestionKey;

    public TrafficCongestionPublisher(IConfiguration configuration, ILogger<TrafficCongestionPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;
        _congestionKey = configuration["RabbitMQ:RoutingKeys:TrafficCongestion"] ?? "traffic.analytics.congestion";
    }

    public async Task PublishCongestionAsync(TrafficCongestionMessage message)
    {
        var routingKey = $"{_congestionKey}.{message.IntersectionId}";
        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "Published Traffic Congestion Alert {AlertId} for Intersection {IntersectionId} with level {Level}",
            message.AlertId, message.IntersectionId, message.CongestionLevel
        );
    }
}