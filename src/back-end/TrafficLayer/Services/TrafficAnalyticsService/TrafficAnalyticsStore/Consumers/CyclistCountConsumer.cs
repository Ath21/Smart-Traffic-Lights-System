using MassTransit;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;

namespace TrafficAnalyticsStore.Consumers;

public class CyclistCountConsumer : IConsumer<CyclistCountMessage>
{
    private readonly ILogger<CyclistCountConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;

    private const string ServiceTag = "[" + nameof(CyclistCountConsumer) + "]";

    public CyclistCountConsumer(
        ILogger<CyclistCountConsumer> logger,
        ITrafficAnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    // sensor.cyclist.request.{intersection_id}
    public async Task Consume(ConsumeContext<CyclistCountMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Cyclist detection at Intersection {IntersectionId}, Count {Count}",
            ServiceTag, msg.IntersectionId, msg.Count);

        var dto = new SummaryDto
        {
            IntersectionId = msg.IntersectionId,
            Date = msg.Timestamp.Date,
            VehicleCount = msg.Count,
            AvgSpeed = 0,
            CongestionLevel = "CyclistFlow"
        };

        await _analyticsService.AddOrUpdateSummaryAsync(dto);
    }
}
