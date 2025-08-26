using MassTransit;
using Microsoft.Extensions.Logging;
using SensorMessages;
using TrafficDataAnalyticsStore.Business;
using TrafficDataAnalyticsStore.Models.Dtos;
using TrafficDataAnalyticsStore.Publishers.Summary;
using TrafficMessages;

namespace TrafficDataAnalyticsStore.Consumers;

public class PedestrianDetectionConsumer : IConsumer<PedestrianDetectionMessage>
{
    private readonly ILogger<PedestrianDetectionConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;
    private readonly ITrafficSummaryPublisher _summaryPublisher;

    public PedestrianDetectionConsumer(
        ILogger<PedestrianDetectionConsumer> logger,
        ITrafficAnalyticsService analyticsService,
        ITrafficSummaryPublisher summaryPublisher)
    {
        _logger = logger;
        _analyticsService = analyticsService;
        _summaryPublisher = summaryPublisher;
    }

    public async Task Consume(ConsumeContext<PedestrianDetectionMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "Pedestrian detection at Intersection {IntersectionId}, Count {Count}",
            msg.IntersectionId, msg.Count);

        var dto = new SummaryDto
        {
            IntersectionId = msg.IntersectionId,
            Date = msg.Timestamp.Date,
            VehicleCount = msg.Count, // treat as pedestrian flow
            AvgSpeed = 0,             // not relevant
            CongestionLevel = "N/A"
        };

        await _analyticsService.AddOrUpdateSummaryAsync(dto);

        var summaryMessage = new TrafficSummaryMessage(
            dto.SummaryId,
            dto.IntersectionId,
            dto.Date,
            dto.AvgSpeed,
            dto.VehicleCount,
            dto.CongestionLevel
        );

        await _summaryPublisher.PublishSummaryAsync(summaryMessage);
    }
}
