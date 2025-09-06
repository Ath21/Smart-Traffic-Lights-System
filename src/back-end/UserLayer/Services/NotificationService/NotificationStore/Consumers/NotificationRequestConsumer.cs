using MassTransit;
using NotificationStore.Business.Notify;
using UserMessages;

namespace NotificationStore.Consumers;

public class NotificationRequestConsumer : IConsumer<UserNotificationRequest>
{
    private readonly ILogger<NotificationRequestConsumer> _logger;
    private readonly INotificationService _notificationService;

    private const string ServiceTag = "[" + nameof(NotificationRequestConsumer) + "]";

    public NotificationRequestConsumer(
        ILogger<NotificationRequestConsumer> logger, 
        INotificationService notificationService)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserNotificationRequest> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Received {Type} for user {UserId}, email {Email}, message: {Message}",
            ServiceTag, msg.Type, msg.UserId, msg.RecipientEmail, msg.Message);

        if (msg.UserId != Guid.Empty && !string.IsNullOrWhiteSpace(msg.RecipientEmail))
        {
            await _notificationService.SendUserNotificationAsync(
                msg.UserId,
                msg.RecipientEmail,
                msg.Message ?? string.Empty,
                msg.Type ?? "General"
            );
        }
        else
        {
            await _notificationService.SendPublicNoticeAsync(
                msg.Type ?? "Notice",
                msg.Message ?? string.Empty,
                msg.TargetAudience ?? "All"
            );
        }
    }
}
