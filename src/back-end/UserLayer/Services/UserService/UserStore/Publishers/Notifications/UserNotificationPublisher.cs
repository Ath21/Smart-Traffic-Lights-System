using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.User;
using UserStore.Publishers.Logs;

namespace UserStore.Publishers.Notifications;

public class UserNotificationPublisher : IUserNotificationPublisher
{
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<UserNotificationPublisher> _logger;
    private readonly IUserLogPublisher _logPublisher;
    private readonly string _routingKey;

    private const string Tag = "[PUBLISHER][USER_NOTIFICATION]";

    public UserNotificationPublisher(
        IPublishEndpoint publisher,
        IConfiguration configuration,
        ILogger<UserNotificationPublisher> logger,
        IUserLogPublisher logPublisher)
    {
        _publisher = publisher;
        _logger = logger;
        _logPublisher = logPublisher;

        _routingKey = configuration["RabbitMQ:RoutingKeys:User:NotificationRequests"] ?? "user.notification.request";
    }

    public async Task PublishSubscriptionRequestAsync(UserNotificationRequest message)
    {
        try
        {
            await _publisher.Publish(message, ctx => ctx.SetRoutingKey(_routingKey));

            _logger.LogInformation("{Tag} Published subscription request for {UserId} ({Email}) → {Intersection}/{Metric}",
                Tag, message.UserId, message.UserEmail, message.Intersection, message.Metric);

            await _logPublisher.PublishAuditAsync(
                source: "user-api",
                messageText: $"{Tag} Sent subscription request to Notification Service",
                category: "Publish",
                data: new Dictionary<string, object>
                {
                    ["userId"] = message.UserId,
                    ["email"] = message.UserEmail,
                    ["intersection"] = message.Intersection,
                    ["metric"] = message.Metric
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Tag} Failed to publish subscription for {UserId} ({Email})",
                Tag, message.UserId, message.UserEmail);

            await _logPublisher.PublishErrorAsync(
                source: "user-api",
                messageText: $"{Tag} Failed to publish subscription request: {ex.Message}",
                data: new Dictionary<string, object>
                {
                    ["exception"] = ex.Message,
                    ["stackTrace"] = ex.StackTrace ?? string.Empty,
                    ["userId"] = message.UserId,
                    ["email"] = message.UserEmail
                });
        }
    }
}
