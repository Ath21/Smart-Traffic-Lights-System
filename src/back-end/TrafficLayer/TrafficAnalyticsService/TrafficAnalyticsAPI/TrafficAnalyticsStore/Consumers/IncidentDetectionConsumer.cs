using MassTransit;
using Microsoft.Extensions.Logging;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;
using TrafficAnalyticsStore.Publishers.Incident;
using TrafficMessages;

namespace TrafficAnalyticsStore.Consumers;

public class IncidentDetectionConsumer : IConsumer<IncidentDetectionMessage>
{
    private readonly ILogger<IncidentDetectionConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;
    private readonly ITrafficIncidentPublisher _incidentPublisher;

    public IncidentDetectionConsumer(
        ILogger<IncidentDetectionConsumer> logger,
        ITrafficAnalyticsService analyticsService,
        ITrafficIncidentPublisher incidentPublisher)
    {
        _logger = logger;
        _analyticsService = analyticsService;
        _incidentPublisher = incidentPublisher;
    }

    public async Task Consume(ConsumeContext<IncidentDetectionMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "Incident detected at Intersection {IntersectionId}: {Description}",
            msg.IntersectionId, msg.Description);

        var dto = new IncidentDto
        {
            AlertId = msg.DetectionId,
            IntersectionId = msg.IntersectionId,
            Type = "Incident",
            Message = msg.Description,
            CreatedAt = msg.Timestamp
        };

        // Persist in DB
        await _analyticsService.ReportIncidentAsync(dto);

        // Publish incident event
        var incidentMessage = new TrafficIncidentMessage(
            dto.AlertId,
            dto.IntersectionId,
            dto.Message,
            dto.CreatedAt
        );
        await _incidentPublisher.PublishIncidentAsync(incidentMessage);
    }
}
