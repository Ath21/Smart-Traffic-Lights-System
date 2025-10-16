using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.User;

namespace NotificationStore.Publishers.Notifications;

public class UserNotificationPublisher : INotificationPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<UserNotificationPublisher> _logger;
    private readonly string _routingPattern;

    public UserNotificationPublisher(IBus bus, IConfiguration config, ILogger<UserNotificationPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        // Pattern: user.notification.{type}
        _routingPattern = config["RabbitMQ:RoutingKeys:User:Notification"]
                          ?? "user.notification.{type}";
    }

    public async Task PublishUserAlertAsync(int userId, string recipientEmail, string type, string message)
    {
        var msg = new UserNotificationMessage
        {
            CorrelationId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "User Layer",
            DestinationLayer = new() { "User Layer" },

            SourceService = "Notification Service",
            DestinationServices = new() { "User Service" },

            NotificationType = type,
            Title = $"{type} Notification",
            Body = message,
            RecipientEmail = recipientEmail,
            Status = "Sent",
        };

        var routingKey = _routingPattern.Replace("{type}", type.ToLowerInvariant());

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[PUBLISHER][USER_NOTIFICATION] Published {Type} for {Email}",
            type, recipientEmail);
    }

    public async Task PublishPublicNoticeAsync(string notificationId, string title, string body, string audience)
    {
        var msg = new UserNotificationMessage
        {
            CorrelationId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "User Layer",
            DestinationLayer = new() { "User Layer" },

            SourceService = "Notification Service",
            DestinationServices = new() { "User Service" },

            NotificationType = "PublicNotice",
            Title = title,
            Body = body,
            RecipientEmail = audience,
            Status = "Broadcasted"
        };

        var routingKey = _routingPattern.Replace("{type}", "public-notice");

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[PUBLISHER][PUBLIC_NOTICE] Published notice {Title} to {Audience}", title, audience);
    }
}
