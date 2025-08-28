using MassTransit;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;

namespace TrafficAnalyticsStore.Consumers;

public class PublicTransportConsumer : IConsumer<PublicTransportMessage>
{
    private readonly ILogger<PublicTransportConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;

    private const string ServiceTag = "[" + nameof(PublicTransportConsumer) + "]";

    public PublicTransportConsumer(
        ILogger<PublicTransportConsumer> logger,
        ITrafficAnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    // sensor.public_transport.request.{intersection_id}
    public async Task Consume(ConsumeContext<PublicTransportMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} PublicTransport request at Intersection {IntersectionId}, Route {RouteId}",
            ServiceTag, msg.IntersectionId, msg.RouteId ?? "N/A");

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
