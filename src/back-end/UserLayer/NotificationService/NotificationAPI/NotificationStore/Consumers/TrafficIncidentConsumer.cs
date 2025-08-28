using MassTransit;
using NotificationStore.Business.Notify;
using NotificationStore.Models.Dtos;
using TrafficMessages;

namespace NotificationStore.Consumers;

public class TrafficIncidentConsumer : IConsumer<TrafficIncidentMessage>
{
    private readonly ILogger<TrafficIncidentConsumer> _logger;
    private readonly INotificationService _notificationService;

    private const string ServiceTag = "[" + nameof(TrafficIncidentConsumer) + "]";

    public TrafficIncidentConsumer(ILogger<TrafficIncidentConsumer> logger, INotificationService notificationService)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficIncidentMessage> context)
    {
        var dto = new NotificationDto
        {
            Type = "TrafficIncident",
            Title = "Traffic Incident Reported",
            Message = context.Message.Description,
            TargetAudience = "EmergencyServices",
            CreatedAt = context.Message.Timestamp,
            Status = "Pending"
        };

        _logger.LogInformation(
            "{Tag} Incident {IncidentId} reported at intersection {IntersectionId}",
            ServiceTag, context.Message.IncidentId, context.Message.IntersectionId);

        await _notificationService.SendNotificationAsync(dto);
    }
}
