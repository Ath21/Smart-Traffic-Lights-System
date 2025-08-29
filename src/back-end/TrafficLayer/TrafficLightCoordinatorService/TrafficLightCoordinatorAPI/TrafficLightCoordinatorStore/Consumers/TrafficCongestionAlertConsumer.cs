using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficLightCoordinatorStore.Business;
using TrafficMessages;

namespace TrafficLightCoordinatorStore.Consumers;

public class TrafficCongestionAlertConsumer : IConsumer<TrafficCongestionMessage>
{
    private readonly ILogger<TrafficCongestionAlertConsumer> _logger;
    private readonly ICoordinatorService _coordinatorService;

    private const string ServiceTag = "[" + nameof(TrafficCongestionAlertConsumer) + "]";

    public TrafficCongestionAlertConsumer(
        ILogger<TrafficCongestionAlertConsumer> logger,
        ICoordinatorService coordinatorService)
    {
        _logger = logger;
        _coordinatorService = coordinatorService;
    }

    // traffic.analytics.congestion.{intersection_id}
    public async Task Consume(ConsumeContext<TrafficCongestionMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Received congestion alert for Intersection {IntersectionId}, Level={Level}, Severity={Severity}, Ts={Ts:O}",
            ServiceTag, msg.IntersectionId, msg.CongestionLevel, msg.Severity, msg.Timestamp);

        await _coordinatorService.HandleCongestionAsync(msg, context.CancellationToken);
    }
}
