/*
 * NotificationStore.Consumers.NotificationRequestConsumer
 *
 * This file is part of the NotificationStore project, which defines a consumer for handling notification requests.
 * The NotificationRequestConsumer class implements the IConsumer interface from MassTransit,
 * allowing it to consume messages of type NotificationRequest.
 * It is responsible for processing incoming notification requests, creating a NotificationDto,
 * and sending notifications through the INotificationService.
 * The consumer logs the notification details and uses dependency injection to access the notification service.
 * It is typically registered in the MassTransit configuration to listen for NotificationRequest messages.
 */
using MassTransit;
using NotificationStore.Business.Notify;
using NotificationStore.Models;
using UserMessages;

namespace NotificationStore.Consumers;

public class NotificationRequestConsumer : IConsumer<NotificationRequest>
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
            Type = context.Message.Type,
            RecipientId = context.Message.RecipientId,
            RecipientEmail = context.Message.RecipientEmail,
            Message = context.Message.Message,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("NotificationsRequestConsumer: Sending notification to user {UserId} at {RecipientEmail} with message: {Message}",
            dto.RecipientId, dto.RecipientEmail, dto.Message);
        
        await _notificationService.SendNotificationAsync(dto);
    }
}
