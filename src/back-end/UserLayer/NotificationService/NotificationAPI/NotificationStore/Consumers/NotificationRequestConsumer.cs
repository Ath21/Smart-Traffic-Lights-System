using MassTransit;
using NotificationStore.Business.Notify;
using NotificationStore.Models;
using UserMessages;

namespace NotificationStore.Consumers;

public class NotificationRequestConsumer : IConsumer<UserNotificationRequest>
{
    private readonly ILogger<NotificationRequestConsumer> _logger;
    private readonly INotificationService _notificationService;

    public NotificationRequestConsumer(ILogger<NotificationRequestConsumer> logger, INotificationService notificationService)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserNotificationRequest> context)
    {
        var dto = new NotificationDto
        {
            Type = context.Message.RequestType,
            Message = $"User Notification Request: {context.Message.RequestType}",
            TargetAudience = context.Message.TargetAudience,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        _logger.LogInformation(
            "NotificationRequestConsumer: Received request {RequestType} for user {UserId}, target {TargetAudience}",
            context.Message.RequestType, context.Message.UserId, context.Message.TargetAudience);

        await _notificationService.SendNotificationAsync(dto);
    }
}
