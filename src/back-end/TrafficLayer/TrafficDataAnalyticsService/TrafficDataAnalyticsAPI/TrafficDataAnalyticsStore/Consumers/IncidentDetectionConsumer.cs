using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace TrafficDataAnalyticsStore.Consumers;

public class IncidentDetectionConsumer : IConsumer<IncidentDetectionMessage>
{
    private readonly ILogger<IncidentDetectionConsumer> _logger;

    public IncidentDetectionConsumer(ILogger<IncidentDetectionConsumer> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Consume(ConsumeContext<IncidentDetectionMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "Incident detected at Intersection {IntersectionId}: {Description}",
            msg.IntersectionId, msg.Description);

        return Task.CompletedTask;
    }
}
