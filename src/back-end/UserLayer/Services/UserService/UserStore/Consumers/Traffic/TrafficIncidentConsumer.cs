using MassTransit;
using TrafficMessages;

namespace UserStore.Consumers.Traffic;

public class TrafficIncidentConsumer : IConsumer<TrafficIncidentMessage>
{
    private readonly ILogger<TrafficIncidentConsumer> _logger;

    private const string ServiceTag = "[" + nameof(TrafficIncidentConsumer) + "]";

    public TrafficIncidentConsumer(ILogger<TrafficIncidentConsumer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // traffic.analytics.incident.{intersection_id}
    public Task Consume(ConsumeContext<TrafficIncidentMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Received TrafficIncident at Intersection {IntersectionId}: {Description}",
            ServiceTag, msg.IntersectionId, msg.Description
        );

        // TODO: store incident or notify operators
        return Task.CompletedTask;
    }
}
