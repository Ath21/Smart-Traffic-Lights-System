using System;
using MassTransit;
using TrafficMessages;

namespace TrafficDataAnalyticsStore.Publishers.Summary;

public class TrafficSummaryPublisher : ITrafficSummaryPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficSummaryPublisher> _logger;
    private readonly string _summaryKey;

    public TrafficSummaryPublisher(IConfiguration configuration, ILogger<TrafficSummaryPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;
        _summaryKey = configuration["RabbitMQ:RoutingKeys:TrafficSummary"] ?? "traffic.analytics.summary";
    }

    public async Task PublishSummaryAsync(TrafficSummaryMessage message)
    {
        var routingKey = $"{_summaryKey}.{message.IntersectionId}";
        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "Published Traffic Summary {SummaryId} for Intersection {IntersectionId} on {Date} (AvgSpeed {Speed}, Count {Count}, Level {Level})",
            message.SummaryId, message.IntersectionId, message.Date, message.AvgSpeed, message.VehicleCount, message.CongestionLevel
        );
    }
}
