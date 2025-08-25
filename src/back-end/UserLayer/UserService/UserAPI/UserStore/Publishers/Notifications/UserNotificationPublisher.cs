using System;
using MassTransit;
using UserMessages;

namespace UserStore.Publishers.Notifications;

public class UserNotificationPublisher : IUserNotificationPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<UserNotificationPublisher> _logger;
    private readonly string _notificationKey;

    public UserNotificationPublisher(IConfiguration configuration, ILogger<UserNotificationPublisher> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
        _notificationKey = configuration["RabbitMQ:RoutingKeys:NotificationsRequest"] ?? "user.notification.request";
    }
    
    public async Task PublishNotificationAsync(
        Guid userId,
        string recipientEmail,
        string type,
        string message,
        string targetAudience)
    {
        var notification = new UserNotificationRequest(
            userId,
            recipientEmail,
            type,
            message,
            targetAudience,
            DateTime.UtcNow
        );

        await _bus.Publish(notification, ctx => ctx.SetRoutingKey(_notificationKey));

        _logger.LogInformation(
            "Notification published for {UserId}, type {Type}, email {Email}, audience {Audience}",
            userId, type, recipientEmail, targetAudience
        );
    }


}