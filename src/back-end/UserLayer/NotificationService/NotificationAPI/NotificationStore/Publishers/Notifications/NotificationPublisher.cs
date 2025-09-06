using MassTransit;
using UserMessages;

namespace NotificationStore.Publishers.Notifications;

public class NotificationPublisher : INotificationPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<NotificationPublisher> _logger;
    private readonly string _publicNoticeKey;
    private readonly string _userAlertKey;

    private const string ServiceTag = "[" + nameof(NotificationPublisher) + "]";

    public NotificationPublisher(IConfiguration configuration, ILogger<NotificationPublisher> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;

        _publicNoticeKey = configuration["RabbitMQ:RoutingKeys:User:NotificationPublic"]
                           ?? "notification.event.public_notice";

        _userAlertKey = configuration["RabbitMQ:RoutingKeys:User:NotificationAlert"]
                        ?? "user.notification.alert";
    }

    // notification.event.public_notice
    public async Task PublishPublicNoticeAsync(Guid noticeId, string title, string message, string targetAudience)
    {
        var evt = new PublicNoticeEvent(
            noticeId,
            title,
            message,
            targetAudience,
            DateTime.UtcNow
        );

        await _bus.Publish(evt, ctx => ctx.SetRoutingKey(_publicNoticeKey));

        _logger.LogInformation("{Tag} Public notice published with Id {NoticeId}, Title {Title}, Audience {Audience}",
            ServiceTag, noticeId, title, targetAudience);
    }

    // user.notification.alert
    public async Task PublishUserAlertAsync(Guid userId, string email, string alertType, string message)
    {
        var alert = new UserNotificationAlert(
            userId,
            email,
            alertType,
            message,
            DateTime.UtcNow
        );

        await _bus.Publish(alert, ctx => ctx.SetRoutingKey(_userAlertKey));

        _logger.LogInformation("{Tag} User alert published for {UserId}, Email {Email}, Type {Type}",
            ServiceTag, userId, email, alertType);
    }
}
