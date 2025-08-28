using MassTransit;
using UserMessages;

namespace UserStore.Consumers.Usr;

public class UserNotificationAlertConsumer : IConsumer<UserNotificationAlert>
{
    private readonly ILogger<UserNotificationAlertConsumer> _logger;

    private const string ServiceTag = "[" + nameof(UserNotificationAlertConsumer) + "]";

    public UserNotificationAlertConsumer(ILogger<UserNotificationAlertConsumer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // user.notification.alert
    public Task Consume(ConsumeContext<UserNotificationAlert> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Received UserNotificationAlert for User {UserId}, Email {Email}, Type {AlertType}, Message {Message}",
            ServiceTag, msg.UserId, msg.Email, msg.AlertType, msg.Message
        );

        // TODO: handle notification persistence or forward to front-end signal
        return Task.CompletedTask;
    }
}
