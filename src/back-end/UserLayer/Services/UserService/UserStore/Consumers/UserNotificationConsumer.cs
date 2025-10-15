using MassTransit;
using Microsoft.Extensions.Logging;
using Messages.User;
using UserStore.Publishers.Logs;

namespace UserStore.Consumers;

public class UserNotificationConsumer : IConsumer<UserNotificationMessage>
{
    private readonly ILogger<UserNotificationConsumer> _logger;
    private readonly IUserLogPublisher _logPublisher;

    public UserNotificationConsumer(
        ILogger<UserNotificationConsumer> logger,
        IUserLogPublisher logPublisher)
    {
        _logger = logger;
        _logPublisher = logPublisher;
    }

    public async Task Consume(ConsumeContext<UserNotificationMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER][NOTIFICATION] {Type} - {Title} ({Status}) for {Recipient}",
            msg.NotificationType,
            msg.Title,
            msg.Status,
            msg.RecipientEmail ?? "broadcast");

        // Log successful or failed delivery
        string action = $"USER_NOTIFICATION_{msg.Status?.ToUpper()}";
        string message = $"{msg.NotificationType} notification '{msg.Title}' processed (Status={msg.Status})";

        await _logPublisher.PublishAuditAsync(action, message);
    }
}
