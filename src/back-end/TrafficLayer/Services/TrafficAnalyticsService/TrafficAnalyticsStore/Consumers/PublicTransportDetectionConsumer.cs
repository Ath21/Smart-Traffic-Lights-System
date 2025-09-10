using MassTransit;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;

namespace TrafficAnalyticsStore.Consumers;

public class PublicTransportDetectionConsumer : IConsumer<PublicTransportDetectionMessage>
{
    private readonly ILogger<PublicTransportDetectionConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;

    private const string ServiceTag = "[" + nameof(PublicTransportDetectionConsumer) + "]";

    public PublicTransportDetectionConsumer(
        ILogger<PublicTransportDetectionConsumer> logger,
        ITrafficAnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    // sensor.public_transport.request.{intersection_id}
    public async Task Consume(ConsumeContext<PublicTransportDetectionMessage> context)
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
