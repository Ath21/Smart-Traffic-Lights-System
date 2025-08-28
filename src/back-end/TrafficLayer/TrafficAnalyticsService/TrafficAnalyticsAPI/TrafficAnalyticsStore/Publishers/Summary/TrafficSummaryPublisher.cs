using MassTransit;
using TrafficMessages;

namespace TrafficAnalyticsStore.Publishers.Summary;

public class TrafficSummaryPublisher : ITrafficSummaryPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficSummaryPublisher> _logger;
    private readonly string _summaryKey;

    private const string ServiceTag = "[" + nameof(TrafficSummaryPublisher) + "]";

    public TrafficSummaryPublisher(IConfiguration configuration, ILogger<TrafficSummaryPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _summaryKey = configuration["RabbitMQ:RoutingKeys:TrafficSummary"] 
                      ?? "traffic.analytics.summary.{intersection_id}";
    }

    // traffic.analytics.summary.{intersection_id}
    public async Task PublishSummaryAsync(TrafficSummaryMessage message)
    {
        var routingKey = _summaryKey.Replace("{intersection_id}", message.IntersectionId.ToString());

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "{Tag} Published Traffic Summary {SummaryId} for Intersection {IntersectionId} on {Date} (AvgSpeed {Speed}, Count {Count}, Level {Level})",
            ServiceTag, message.SummaryId, message.IntersectionId, message.Date, message.AvgSpeed, message.VehicleCount, message.CongestionLevel
        );
    }
}
