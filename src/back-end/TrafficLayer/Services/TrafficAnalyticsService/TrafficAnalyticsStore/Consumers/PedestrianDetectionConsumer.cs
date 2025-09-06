using MassTransit;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;

namespace TrafficAnalyticsStore.Consumers;

public class PedestrianDetectionConsumer : IConsumer<PedestrianDetectionMessage>
{
    private readonly ILogger<PedestrianDetectionConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;

    private const string ServiceTag = "[" + nameof(PedestrianDetectionConsumer) + "]";

    public PedestrianDetectionConsumer(
        ILogger<PedestrianDetectionConsumer> logger,
        ITrafficAnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    // sensor.pedestrian.request.{intersection_id}
    public async Task Consume(ConsumeContext<PedestrianDetectionMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Pedestrian detection at Intersection {IntersectionId}, Count {Count}",
            ServiceTag, msg.IntersectionId, msg.Count);

        var dto = new SummaryDto
        {
            IntersectionId = msg.IntersectionId,
            Date = msg.Timestamp.Date,
            VehicleCount = msg.Count,
            AvgSpeed = 0,
            CongestionLevel = "PedestrianFlow"
        };

        await _analyticsService.AddOrUpdateSummaryAsync(dto);
    }
}
