using System;
using MassTransit;
using Messages.Traffic;

namespace NotificationStore.Consumers;

public class TrafficAnalyticsConsumer : IConsumer<TrafficAnalyticsMessage>
{
    private readonly ILogger<TrafficAnalyticsConsumer> _logger;
    private readonly IUserNotificationPublisher _notificationPublisher;
    private readonly IUserLogPublisher _logPublisher;

    public TrafficAnalyticsConsumer(
        ILogger<TrafficAnalyticsConsumer> logger,
        IUserNotificationPublisher notificationPublisher,
        IUserLogPublisher logPublisher)
    {
        _logger = logger;
        _notificationPublisher = notificationPublisher;
        _logPublisher = logPublisher;
    }

    public async Task Consume(ConsumeContext<TrafficAnalyticsMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER][ANALYTICS][{Intersection}] {MetricType} metric received (CI={CI:F2}, Severity={Severity})",
            msg.IntersectionName, msg.MetricType, msg.CongestionIndex, msg.Severity);

        // Basic business routing for user notifications
        string title;
        string body;
        string notificationType;

        switch (msg.MetricType?.ToLowerInvariant())
        {
            case "incident":
                title = $"⚠️ Traffic Incident at {msg.IntersectionName}";
                body = $"An incident was detected. Severity level: {msg.Severity}. Expect delays.";
                notificationType = "Alert";
                break;

            case "congestion":
                title = $"🚗 Heavy Congestion at {msg.IntersectionName}";
                body = $"Congestion Index: {msg.CongestionIndex:F2} (Severity {msg.Severity}). Consider alternate routes.";
                notificationType = "PublicNotice";
                break;

            default:
                title = $"ℹ️ Traffic Summary Update - {msg.IntersectionName}";
                body = $"Average Speed: {msg.AverageSpeedKmh:F1} km/h, Wait Time: {msg.AverageWaitTimeSec:F1} s.";
                notificationType = "PublicNotice";
                break;
        }

        // Send the notification
        await _notificationPublisher.PublishNotificationAsync(
            notificationType,
            title,
            body,
            recipientEmail: "public@stls.local", // system broadcast; in real case: targeted users
            status: "Pending",
            correlationId: msg.CorrelationId);

        // Log the event
        await _logPublisher.PublishAuditAsync(
            action: $"TRAFFIC_ANALYTICS_{msg.MetricType?.ToUpper()}",
            message: $"Received traffic analytics event for {msg.IntersectionName} (Severity {msg.Severity})",
            metadata: new Dictionary<string, string?>
            {
                ["intersection"] = msg.IntersectionName,
                ["metric_type"] = msg.MetricType,
                ["severity"] = msg.Severity.ToString()
            }!);
    }
}