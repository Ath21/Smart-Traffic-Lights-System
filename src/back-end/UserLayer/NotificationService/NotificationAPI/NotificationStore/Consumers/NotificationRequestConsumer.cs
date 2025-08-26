using MassTransit;
using NotificationStore.Business.Notify;
using UserMessages;

namespace NotificationStore.Consumers;

public class NotificationRequestConsumer : IConsumer<UserNotificationRequest>
{
    private readonly ILogger<NotificationRequestConsumer> _logger;
    private readonly INotificationService _notificationService;

    public NotificationRequestConsumer(
        ILogger<NotificationRequestConsumer> logger, 
        INotificationService notificationService)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserNotificationRequest> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "NotificationRequestConsumer: Received {Type} for user {UserId}, email {Email}, message: {Message}",
            message.Type, message.UserId, message.RecipientEmail, message.Message);

        if (message.UserId != Guid.Empty && !string.IsNullOrWhiteSpace(message.RecipientEmail))
        {
            // Targeted user notification
            await _notificationService.SendUserNotificationAsync(
                message.UserId,
                message.RecipientEmail,
                message.Message ?? string.Empty,
                message.Type ?? "General"
            );
        }
        else
        {
            // Public broadcast notice
            await _notificationService.SendPublicNoticeAsync(
                message.Type ?? "Notice",
                message.Message ?? string.Empty,
                message.TargetAudience ?? "All"
            );
        }
    }
}
