using MassTransit;

using Messages.User;

namespace UserStore.Publishers.Notifications;

public class UserNotificationPublisher : IUserNotificationPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<UserNotificationPublisher> _logger;
    private readonly string _routingPattern;

    public UserNotificationPublisher(
        IConfiguration config,
        ILogger<UserNotificationPublisher> logger,
        IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _routingPattern = config["RabbitMQ:RoutingKeys:User:Notification"]
                          ?? "user.notification.{type}";
    }

    // ============================================================
    // Publish REQUEST (User action request)
    // ============================================================
    public async Task PublishRequestAsync(
        string title,
        string body,
        string recipientEmail,
        string status = "Pending",
        Guid? correlationId = null)
    {
        await PublishAsync("request", new UserNotificationMessage
        {
            NotificationType = "Request",
            Title = title,
            Body = body,
            RecipientEmail = recipientEmail,
            Status = status,
            SourceServices = new() { "User Service" },
            DestinationServices = new() { "Notification Service" }
        }, correlationId);

        _logger.LogInformation("REQUEST notification sent to {Recipient}", recipientEmail);
    }

    // ============================================================
    // Internal Publish Logic
    // ============================================================
    private async Task PublishAsync(string type, UserNotificationMessage msg, Guid? correlationId)
    {
        msg.CorrelationId = correlationId ?? Guid.NewGuid();
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern.Replace("{type}", type);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
