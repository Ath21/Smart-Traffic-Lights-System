using MassTransit;
using TrafficMessages;

namespace UserStore.Consumers.Traffic;

public class TrafficSummaryConsumer : IConsumer<TrafficSummaryMessage>
{
    private readonly ILogger<TrafficSummaryConsumer> _logger;

    private const string ServiceTag = "[" + nameof(TrafficSummaryConsumer) + "]";

    public TrafficSummaryConsumer(ILogger<TrafficSummaryConsumer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // traffic.analytics.summary.{intersection_id}
    public Task Consume(ConsumeContext<TrafficSummaryMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Received TrafficSummary for Intersection {IntersectionId} - {VehicleCount} vehicles, AvgSpeed {AvgSpeed}, Congestion {Level}",
            ServiceTag, msg.IntersectionId, msg.VehicleCount, msg.AvgSpeed, msg.CongestionLevel
        );

        // TODO: persist or visualize in dashboards
        return Task.CompletedTask;
    }
}
