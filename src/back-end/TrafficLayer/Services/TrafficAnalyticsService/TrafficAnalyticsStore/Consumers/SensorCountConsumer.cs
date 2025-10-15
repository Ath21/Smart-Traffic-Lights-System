using MassTransit;
using Messages.Sensor;
using TrafficAnalyticsStore.Aggregators;

namespace TrafficAnalyticsStore.Consumers;

public class SensorCountConsumer : IConsumer<SensorCountMessage>
{
    private readonly ILogger<SensorCountConsumer> _logger;
    private readonly ITrafficAnalyticsAggregator _aggregator;

    public SensorCountConsumer(ILogger<SensorCountConsumer> logger, ITrafficAnalyticsAggregator aggregator)
    {
        _logger = logger;
        _aggregator = aggregator;
    }

    public async Task Consume(ConsumeContext<SensorCountMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER][COUNT][{Intersection}] {Type} count received: {Count} (Speed={Speed:F1} km/h, Wait={Wait:F1}s, Flow={Flow:F2}/s)",
            msg.IntersectionName,
            msg.CountType,
            msg.Count,
            msg.AverageSpeedKmh,
            msg.AverageWaitTimeSec,
            msg.FlowRate);

        try
        {
            await _aggregator.UpdateCountAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CONSUMER][COUNT][{Intersection}] Failed to process count message", msg.IntersectionName);
            throw; // Let MassTransit handle retries / dead-letter
        }
    }
}
