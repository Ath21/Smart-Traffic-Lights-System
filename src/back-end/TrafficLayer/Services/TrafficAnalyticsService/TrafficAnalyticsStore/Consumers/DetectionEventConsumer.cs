using MassTransit;
using Messages.Sensor;
using TrafficAnalyticsStore.Aggregators;

namespace TrafficAnalyticsStore.Consumers;

public class DetectionEventConsumer : IConsumer<DetectionEventMessage>
{
    private readonly ILogger<DetectionEventConsumer> _logger;
    private readonly ITrafficAnalyticsAggregator _aggregator;

    public DetectionEventConsumer(ILogger<DetectionEventConsumer> logger, ITrafficAnalyticsAggregator aggregator)
    {
        _logger = logger;
        _aggregator = aggregator;
    }

    public async Task Consume(ConsumeContext<DetectionEventMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER][EVENT][{Intersection}] {EventType} detected ({VehicleType}, Dir={Direction})",
            msg.IntersectionName,
            msg.EventType,
            msg.VehicleType,
            msg.Direction);

        try
        {
            await _aggregator.UpdateEventAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CONSUMER][EVENT][{Intersection}] Failed to process detection event", msg.IntersectionName);
            throw;
        }
    }
}
