using MassTransit;
using Messages.User;
using Microsoft.Extensions.Logging;
using NotificationService.Services;

namespace NotificationStore.Consumers;

public class UserNotificationConsumer : IConsumer<UserNotificationMessage>
{
    private readonly INotificationProcessor _processor;
    private readonly ILogger<UserNotificationConsumer> _logger;

    public UserNotificationConsumer(
        INotificationProcessor processor,
        ILogger<UserNotificationConsumer> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserNotificationMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation("📨 [CONSUMER][NOTIFICATION] Received '{Type}' message for {Recipient}",
            msg.NotificationType, msg.RecipientEmail);

        try
        {
            await _processor.ProcessNotificationAsync(msg);
            _logger.LogInformation("✅ Notification processed successfully for {Recipient}", msg.RecipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to process notification for {Recipient}", msg.RecipientEmail);
            throw;
        }
    }
}
