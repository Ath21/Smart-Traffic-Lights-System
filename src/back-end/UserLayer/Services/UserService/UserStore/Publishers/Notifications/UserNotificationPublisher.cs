using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.User;

namespace UserStore.Publishers.Notifications;

public class UserNotificationPublisher : IUserNotificationPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<UserNotificationPublisher> _logger;
    private readonly string _routingPattern;

    public UserNotificationPublisher(
        IBus bus,
        IConfiguration config,
        ILogger<UserNotificationPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        _routingPattern = config["RabbitMQ:RoutingKeys:User:NotificationRequests"]
                        ?? "user.notification.request";
    }

    public async Task PublishNotificationRequestAsync(
        string username,
        string status = "Pending",
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var msg = new UserNotificationMessage
        {
            CorrelationId = correlationId ?? Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "User Layer",
            DestinationLayer = new() { "User Layer" },

            SourceService = "User Service",
            DestinationServices = new() { "Notification Service" },

            NotificationType = "Request",
            Title = $"<{correlationId}> Notification Request",
            Body = $"User '{username}' wants to subscribe to traffic notifications.",
            RecipientEmail = "ice19390005@gmail.com",
            Status = status,
            Metadata = metadata
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(_routingPattern));

        _logger.LogInformation("[PUBLISHER][NOTIFICATION] Sent '{Type}' notification to {Recipient}",
            "Request", "ice19390005@gmail.com");
    }
}
