using MassTransit;
using Microsoft.Extensions.Logging;
using SensorMessages;
using TrafficAnalyticsStore.Models.Dtos;
using TrafficMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Publishers.Summary;

namespace TrafficAnalyticsStore.Consumers;

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
            VehicleCount = 1,
            AvgSpeed = 0,
            CongestionLevel = "PublicTransport"
        };

        await _analyticsService.AddOrUpdateSummaryAsync(dto);
    }
}
