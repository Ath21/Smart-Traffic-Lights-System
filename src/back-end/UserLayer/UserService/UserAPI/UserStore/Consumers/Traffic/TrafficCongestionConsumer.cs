using MassTransit;
using TrafficMessages;

namespace UserStore.Consumers.Traffic;

public class TrafficCongestionConsumer : IConsumer<TrafficCongestionMessage>
{
    private readonly ILogger<TrafficCongestionConsumer> _logger;

    private const string ServiceTag = "[" + nameof(TrafficCongestionConsumer) + "]";

    public TrafficCongestionConsumer(ILogger<TrafficCongestionConsumer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // traffic.analytics.congestion.{intersection_id}
    public Task Consume(ConsumeContext<TrafficCongestionMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Received TrafficCongestion at Intersection {IntersectionId} - Level {Level}: {Message}",
            ServiceTag, msg.IntersectionId, msg.CongestionLevel, msg.Message
        );

        // TODO: trigger UI update or alert
        return Task.CompletedTask;
    }
}