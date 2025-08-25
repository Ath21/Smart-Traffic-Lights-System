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
            Type = context.Message.Type,
            Message = context.Message.Message,               // correct message text
            TargetAudience = context.Message.TargetAudience, // "User"
            RecipientEmail = context.Message.RecipientEmail, // actual email
            CreatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        _logger.LogInformation(
            "NotificationRequestConsumer: Received {Type} for user {UserId}, email {Email}, message: {Message}",
            dto.Type, context.Message.UserId, dto.RecipientEmail, dto.Message);

        await _notificationService.SendNotificationAsync(dto);
    }


}
