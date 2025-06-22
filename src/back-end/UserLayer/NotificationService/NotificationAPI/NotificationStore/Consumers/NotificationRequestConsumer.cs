using System;
using MassTransit;
using NotificationStore.Business.Notify;
using NotificationStore.Models;
using UserMessages;

namespace NotificationStore.Consumers;

public class NotificationRequestConsumer
{
    private readonly ILogger<NotificationRequestConsumer> _logger;
    private readonly INotificationService _notificationService;

    public NotificationRequestConsumer(ILogger<NotificationRequestConsumer> logger, INotificationService notificationService)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<NotificationRequest> context)
    {
        var dto = new NotificationDto
        {
            Type = "User Notification Request",
            RecipientId = context.Message.UserId,
            RecipientEmail = context.Message.Email,
            Message = context.Message.Message,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("NotificationRequestConsumer: Sending notification to user {UserId} with message: {Message}",
            dto.RecipientId, dto.Message);
        
        await _notificationService.CreateAsync(dto);
    }
}
