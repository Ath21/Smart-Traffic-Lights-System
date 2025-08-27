using MassTransit;
using Microsoft.Extensions.Logging;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;
using TrafficAnalyticsStore.Publishers.Summary;
using TrafficMessages;

namespace TrafficAnalyticsStore.Consumers;

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
            VehicleCount = msg.Count,
            AvgSpeed = 0,
            CongestionLevel = "PedestrianFlow"
        };

        await _analyticsService.AddOrUpdateSummaryAsync(dto);
    }
}
