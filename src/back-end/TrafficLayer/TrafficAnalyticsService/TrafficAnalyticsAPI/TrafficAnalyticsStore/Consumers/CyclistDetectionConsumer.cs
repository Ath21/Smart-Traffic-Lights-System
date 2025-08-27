using MassTransit;
using Microsoft.Extensions.Logging;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;
using TrafficAnalyticsStore.Publishers.Summary;
using TrafficMessages;

namespace TrafficAnalyticsStore.Consumers;

public class CyclistDetectionConsumer : IConsumer<CyclistDetectionMessage>
{
    private readonly ILogger<CyclistDetectionConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;
    private readonly ITrafficSummaryPublisher _summaryPublisher;

    public CyclistDetectionConsumer(
        ILogger<CyclistDetectionConsumer> logger,
        ITrafficAnalyticsService analyticsService,
        ITrafficSummaryPublisher summaryPublisher)
    {
        _logger = logger;
        _analyticsService = analyticsService;
        _summaryPublisher = summaryPublisher;
    }

    public async Task Consume(ConsumeContext<CyclistDetectionMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "Cyclist detection at Intersection {IntersectionId}, Count {Count}",
            msg.IntersectionId, msg.Count);

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
