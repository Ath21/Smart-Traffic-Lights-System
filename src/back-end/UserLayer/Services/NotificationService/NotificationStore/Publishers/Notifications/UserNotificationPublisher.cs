using MassTransit;

using Messages.User;

namespace NotificationStore.Publishers.Notifications;

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
    // Publish ALERT (Traffic-related, broadcast)
    // ============================================================
    public async Task PublishAlertAsync(
        string title,
        string body,
        string recipientEmail = "all@uniwa-stls",
        string status = "Broadcasted",
        int intersectionId = 0,
        string? intersectionName = null,
        Guid? correlationId = null)
    {
        await PublishAsync("alert", new UserNotificationMessage
        {
            NotificationType = "Alert",
            Title = title,
            Body = body,
            RecipientEmail = recipientEmail,
            Status = status,
            IntersectionId = intersectionId,
            IntersectionName = intersectionName,
            SourceServices = new() { "Notification Service" },
            DestinationServices = new() { "User Service" }
        }, correlationId);

        _logger.LogInformation("ALERT notification published → {Title}", title);
    }

    // ============================================================
    // Publish PUBLIC NOTICE (System announcements)
    // ============================================================
    public async Task PublishPublicNoticeAsync(
        string title,
        string body,
        string recipientEmail = "all@uniwa-stls",
        string status = "Broadcasted",
        Guid? correlationId = null)
    {
        await PublishAsync("public-notice", new UserNotificationMessage
        {
            NotificationType = "PublicNotice",
            Title = title,
            Body = body,
            RecipientEmail = recipientEmail,
            Status = status,
            SourceServices = new() { "Notification Service" },
            DestinationServices = new() { "User Service" }
        }, correlationId);

        _logger.LogInformation("PUBLIC NOTICE published → {Title}", title);
    }

    // ============================================================
    // Publish PRIVATE MESSAGE (Directed user communication)
    // ============================================================
    public async Task PublishPrivateAsync(
        string title,
        string body,
        string recipientEmail,
        string status = "Sent",
        int intersectionId = 0,
        string? intersectionName = null,
        Guid? correlationId = null)
    {
        await PublishAsync("private", new UserNotificationMessage
        {
            NotificationType = "Private",
            Title = title,
            Body = body,
            RecipientEmail = recipientEmail,
            Status = status,
            IntersectionId = intersectionId,
            IntersectionName = intersectionName,
            SourceServices = new() { "Notification Service" },
            DestinationServices = new() { "User Service" }
        }, correlationId);

        _logger.LogInformation("PRIVATE notification sent to {Recipient}", recipientEmail);
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
            SourceServices = new() { "Notification Service" },
            DestinationServices = new() { "User Service" }
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
