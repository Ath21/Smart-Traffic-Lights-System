using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficLightCoordinatorStore.Business;
using TrafficLightCoordinatorStore.Business.Coordination;
using TrafficLightCoordinatorStore.Models.Dtos;
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
            "{Tag} Received congestion alert for Intersection {IntersectionId}, Level={Level}, Message={Message}, Ts={Ts:O}",
            ServiceTag, msg.IntersectionId, msg.CongestionLevel, msg.Message, msg.Timestamp);

        // Map message → DTO
        var dto = new CongestionDto
        {
            IntersectionId  = msg.IntersectionId,
            CongestionLevel = ParseLevel(msg.CongestionLevel),
            Severity        = msg.CongestionLevel,
            AppliedPattern  = PatternBuilder.For("congestion",
                                string.Equals(msg.CongestionLevel, "High", StringComparison.OrdinalIgnoreCase)),
            AppliedAt       = DateTimeOffset.UtcNow
        };

        await _coordinatorService.HandleCongestionAsync(dto, context.CancellationToken);
    }

    private static double ParseLevel(string level)
    {
        // Try to parse CongestionLevel string to a numeric value if possible
        if (double.TryParse(level, out var numeric))
            return numeric;

        // Otherwise, map "Low", "Medium", "High" to a scale
        return level.ToLowerInvariant() switch
        {
            "low"    => 0.25,
            "medium" => 0.5,
            "high"   => 0.9,
            _        => 0.0
        };
    }
}
