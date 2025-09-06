using MassTransit;
using SensorMessages;
using TrafficAnalyticsStore.Business;
using TrafficAnalyticsStore.Models.Dtos;

namespace TrafficAnalyticsStore.Consumers;

public class EmergencyVehicleConsumer : IConsumer<EmergencyVehicleMessage>
{
    private readonly ILogger<EmergencyVehicleConsumer> _logger;
    private readonly ITrafficAnalyticsService _analyticsService;

    private const string ServiceTag = "[" + nameof(EmergencyVehicleConsumer) + "]";

    public EmergencyVehicleConsumer(
        ILogger<EmergencyVehicleConsumer> logger,
        ITrafficAnalyticsService analyticsService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
    }

    // sensor.vehicle.emergency.{intersection_id}
    public async Task Consume(ConsumeContext<EmergencyVehicleMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Emergency vehicle {Detected} at Intersection {IntersectionId}",
            ServiceTag, msg.Detected ? "DETECTED" : "Not Detected", msg.IntersectionId);

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
