using System;
using MassTransit;
using NotificationStore.Models;
using UserMessages;

namespace NotificationStore.Consumers;

public class NotificationRequestConsumer
{
    private readonly ILogger<NotificationRequestConsumer> _logger;

    public NotificationRequestConsumer(ILogger<NotificationRequestConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<NotificationRequest> context)
    {
        var dto = new NotificationDto
        {
            RecipientId = context.Message.UserId,
            Message = context.Message.Message,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("NotificationRequestConsumer: Sending notification to user {UserId} with message: {Message}", 
            dto.RecipientId, dto.Message);
    }
}
