using MassTransit;
using Microsoft.Extensions.Logging;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;
using TrafficAnalyticsStore.Publishers.Incident;
using TrafficMessages;

namespace TrafficAnalyticsStore.Consumers;

public class EmergencyVehicleConsumer : IConsumer<EmergencyVehicleMessage>
{
    private readonly ILogger<EmergencyVehicleConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;
    private readonly ITrafficIncidentPublisher _incidentPublisher;

    public EmergencyVehicleConsumer(
        ILogger<EmergencyVehicleConsumer> logger,
        ITrafficAnalyticsService analyticsService,
        ITrafficIncidentPublisher incidentPublisher)
    {
        _logger = logger;
        _analyticsService = analyticsService;
        _incidentPublisher = incidentPublisher;
    }


    public async Task Consume(ConsumeContext<EmergencyVehicleMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "Emergency vehicle {Detected} at Intersection {IntersectionId}",
            msg.Detected ? "DETECTED" : "Not Detected", msg.IntersectionId);

        if (msg.Detected)
        {
            var dto = new IncidentDto
            {
                AlertId = msg.DetectionId,
                IntersectionId = msg.IntersectionId,
                Type = "EmergencyVehicle",
                Message = "Emergency vehicle detected",
                CreatedAt = msg.Timestamp
            };

            await _analyticsService.ReportIncidentAsync(dto);
        }
    }
}
