using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace TrafficDataAnalyticsStore.Consumers;

public class PedestrianDetectionConsumer : IConsumer<PedestrianDetectionMessage>
{
    private readonly ILogger<PedestrianDetectionConsumer> _logger;

    public PedestrianDetectionConsumer(ILogger<PedestrianDetectionConsumer> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Consume(ConsumeContext<PedestrianDetectionMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "Pedestrian request at Intersection {IntersectionId}, Count {Count}",
            msg.IntersectionId, msg.Count);

        return Task.CompletedTask;
    }
}
