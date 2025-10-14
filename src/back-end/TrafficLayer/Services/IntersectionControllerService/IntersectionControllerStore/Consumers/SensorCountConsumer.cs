using MassTransit;
using Microsoft.Extensions.Logging;
using Messages.Sensor;

namespace IntersectionControllerStore.Consumers;

public class SensorCountConsumer : IConsumer<SensorCountMessage>
{
    private readonly ILogger<SensorCountConsumer> _logger;

    public SensorCountConsumer(ILogger<SensorCountConsumer> logger)
    {
        _logger = logger;
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

        // Example business logic:
        // - Update in-memory intersection state
        // - Trigger adaptive signal control
        // - Notify analytics service

        await Task.CompletedTask;
    }
}
