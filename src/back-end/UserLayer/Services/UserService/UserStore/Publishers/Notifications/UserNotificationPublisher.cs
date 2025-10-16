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

        _routingPattern = config["RabbitMQ:RoutingKeys:User:Notification"]
                          ?? "user.notification.{type}";
    }

    public async Task PublishNotificationRequestAsync(
        string title,
        string body,
        string recipientEmail,
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

            NotificationType = "request",
            Title = title,
            Body = body,
            RecipientEmail = recipientEmail,
            Status = status,
            Metadata = metadata
        };

        var routingKey = _routingPattern.Replace("{type}", "request");

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[PUBLISHER][NOTIFICATION] Sent '{Type}' notification to {Recipient}",
            "request", recipientEmail);
    }
}
