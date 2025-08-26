using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace TrafficDataAnalyticsStore.Consumers;

public class CyclistDetectionConsumer : IConsumer<CyclistDetectionMessage>
{
    private readonly ILogger<CyclistDetectionConsumer> _logger;

    public CyclistDetectionConsumer(ILogger<CyclistDetectionConsumer> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Consume(ConsumeContext<CyclistDetectionMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "Cyclist request at Intersection {IntersectionId}, Count {Count}",
            msg.IntersectionId, msg.Count);

        return Task.CompletedTask;
    }
}
