using MassTransit;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;

namespace TrafficAnalyticsStore.Consumers;

public class IncidentDetectionConsumer : IConsumer<IncidentDetectionMessage>
{
    private readonly ILogger<IncidentDetectionConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;

    private const string ServiceTag = "[" + nameof(IncidentDetectionConsumer) + "]";

    public IncidentDetectionConsumer(
        ILogger<IncidentDetectionConsumer> logger,
        ITrafficAnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    // sensor.incident.detected.{intersection_id}
    public async Task Consume(ConsumeContext<IncidentDetectionMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Incident detected at Intersection {IntersectionId}: {Description}",
            ServiceTag, msg.IntersectionId, msg.Description);

        var dto = new IncidentDto
        {
            AlertId = msg.DetectionId,
            IntersectionId = msg.IntersectionId,
            Type = "Incident",
            Message = msg.Description,
            CreatedAt = msg.Timestamp
        };

        await _analyticsService.ReportIncidentAsync(dto);
    }
}
