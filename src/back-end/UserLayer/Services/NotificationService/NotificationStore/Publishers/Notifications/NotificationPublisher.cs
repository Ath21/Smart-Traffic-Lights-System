using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.User;
using NotificationStore.Publishers.Logs;

namespace NotificationStore.Publishers.Notifications;

public class NotificationPublisher : INotificationPublisher
{
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<NotificationPublisher> _logger;
    private readonly ILogPublisher _logPublisher;
    private readonly string _exchange;
    private readonly string _routingPattern;

    private const string ServiceTag = "[PUBLISHER][NOTIFICATION]";

    public NotificationPublisher(
        IPublishEndpoint publisher,
        IConfiguration configuration,
        ILogger<NotificationPublisher> logger,
        ILogPublisher logPublisher)
    {
        _publisher = publisher;
        _logger = logger;
        _logPublisher = logPublisher;

        _exchange = configuration["RabbitMQ:Exchanges:User"] ?? "USER.EXCHANGE";
        _routingPattern = configuration["RabbitMQ:RoutingKeys:User:Notifications"] ?? "user.notification.{type}";
    }

    public async Task PublishNotificationAsync(UserNotificationMessage message, string routingKey)
    {
        try
        {
            var finalRoutingKey = routingKey
                .Replace("{type}", message.Type ?? "public")
                .ToLowerInvariant();

            await _publisher.Publish(message, ctx =>
            {
                ctx.SetRoutingKey(finalRoutingKey);
            });

            _logger.LogInformation("{Tag} Published {Type} notification for {UserId}/{UserEmail} via {RoutingKey}",
                ServiceTag, message.Type, message.UserId, message.UserEmail, finalRoutingKey);

            await _logPublisher.PublishAuditAsync(
                "Publisher",
                $"{ServiceTag} Published {message.Type} notification via {finalRoutingKey}",
                data: new()
                {
                    { "userId", message.UserId },
                    { "email", message.UserEmail },
                    { "title", message.Title },
                    { "routingKey", finalRoutingKey }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Tag} Failed to publish notification for {UserId}/{UserEmail}",
                ServiceTag, message.UserId, message.UserEmail);

            await _logPublisher.PublishErrorAsync(
                "Publisher",
                $"{ServiceTag} Failed to publish notification for {message.UserId} ({message.UserEmail}): {ex.Message}",
                data: new()
                {
                    { "title", message.Title },
                    { "exception", ex.Message },
                    { "stack_trace", ex.StackTrace ?? string.Empty }
                });
        }
    }
}
