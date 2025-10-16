using System;
using MassTransit;
using Messages.User;
using NotificationStore.Business;

namespace NotificationStore.Consumers;

public class UserNotificationConsumer : IConsumer<UserNotificationMessage>
{
    private readonly INotificationProcessor _processor;
    private readonly ILogger<UserNotificationConsumer> _logger;

    public UserNotificationConsumer(INotificationProcessor processor, ILogger<UserNotificationConsumer> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserNotificationMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("[CONSUMER][USER_NOTIFICATION] Received {Type} notification for {Email}",
            msg.NotificationType, msg.RecipientEmail);

        try
        {
            await _processor.HandleUserNotificationAsync(msg);
            _logger.LogInformation("User notification processed successfully for {Email}", msg.RecipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user notification for {Email}", msg.RecipientEmail);
        }
    }
}