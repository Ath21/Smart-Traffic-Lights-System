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

    public async Task PublishNotificationAsync(Guid userId, string requestType, string targetAudience)
    {
        var message = new UserNotificationRequest(
            userId,
            requestType,
            targetAudience,
            DateTime.UtcNow
        );

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(_notificationKey));

        _logger.LogInformation("Notification request published for {UserId}, type {RequestType}", userId, requestType);
    }
}