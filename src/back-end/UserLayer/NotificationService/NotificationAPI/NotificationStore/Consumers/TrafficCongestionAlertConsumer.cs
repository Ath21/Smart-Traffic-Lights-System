using MassTransit;
using NotificationStore.Models;
using NotificationStore.Business.Notify;
using TrafficMessages;

public class TrafficCongestionAlertConsumer : IConsumer<TrafficCongestionAlert>
{
    private readonly INotificationService _notificationService;

    public TrafficCongestionAlertConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<TrafficCongestionAlert> context)
    {
        var alert = context.Message;

        var dto = new NotificationDto
        {
            Type = "ALERT",
            RecipientId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            RecipientEmail = "admin@system.local",
            Message = $"⚠️ {alert.Severity} congestion at {alert.IntersectionId}: {alert.Description}",
            Timestamp = alert.Timestamp
        };

        await _notificationService.CreateAsync(dto);
        await _notificationService.SendNotificationAsync(dto);
    }
}
