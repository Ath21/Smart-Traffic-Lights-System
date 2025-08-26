using MassTransit;
using Microsoft.Extensions.Logging;
using SensorMessages;
using TrafficDataAnalyticsStore.Models.Dtos;
using TrafficMessages;
using TrafficDataAnalyticsStore.Business;
using TrafficDataAnalyticsStore.Publishers.Summary;

namespace TrafficDataAnalyticsStore.Consumers;

public class PublicTransportConsumer : IConsumer<PublicTransportMessage>
{
    private readonly ILogger<PublicTransportConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;
    private readonly ITrafficSummaryPublisher _summaryPublisher;

    public PublicTransportConsumer(
        ILogger<PublicTransportConsumer> logger,
        ITrafficAnalyticsService analyticsService,
        ITrafficSummaryPublisher summaryPublisher)
    {
        _logger = logger;
        _analyticsService = analyticsService;
        _summaryPublisher = summaryPublisher;
    }

    public async Task Consume(ConsumeContext<PublicTransportMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "PublicTransport request at Intersection {IntersectionId}, Route {RouteId}",
            msg.IntersectionId, msg.RouteId ?? "N/A");

        var dto = new SummaryDto
        {
            IntersectionId = msg.IntersectionId,
            Date = msg.Timestamp.Date,
            VehicleCount = 1,  // treat each request as one PT unit
            AvgSpeed = 0,
            CongestionLevel = "PublicTransport"
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
