using Messages.Traffic;
using Messages.User;
using NotificationStore.Business.Email;

namespace NotificationStore.Business.MessageHandler;

public class NotificationProcessor : INotificationProcessor
{
    private readonly IEmailService _emailSender;
    private readonly ILogger<NotificationProcessor> _logger;

    public NotificationProcessor(IEmailService emailSender, ILogger<NotificationProcessor> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task HandleUserNotificationAsync(UserNotificationMessage msg)
    {
        var subject = $"[{msg.NotificationType}] {msg.Title}";
        await _emailSender.SendEmailAsync(msg.RecipientEmail!, subject, msg.Body ?? "");
    }

    public async Task HandleTrafficAnalyticsAsync(TrafficAnalyticsMessage msg)
    {
        // Handle based on metric type and severity
        switch (msg.MetricType?.ToLowerInvariant())
        {
            case "incident":
                await _emailSender.SendEmailAsync(
                    "traffic-ops@uniwa.gr",
                    $"INCIDENT at {msg.IntersectionName}",
                    $"Severity {msg.Severity}/5 — Average speed {msg.AverageSpeedKmh:F1} km/h, " +
                    $"Wait {msg.AverageWaitTimeSec:F1}s, Congestion {msg.CongestionIndex:P1}");
                break;

            case "congestion":
                if (msg.CongestionIndex >= 0.75)
                {
                    await _emailSender.SendEmailAsync(
                        "traffic-alerts@uniwa.gr",
                        $"Congestion at {msg.IntersectionName}",
                        $"Congestion index {msg.CongestionIndex:P1}, average wait {msg.AverageWaitTimeSec:F1}s.");
                }
                break;

            case "summary":
                _logger.LogInformation("Summary: {Intersection} - Avg Speed {Speed:F1} km/h, Vehicles {Count}",
                    msg.IntersectionName, msg.AverageSpeedKmh, msg.TotalVehicleCount);
                break;

            default:
                _logger.LogWarning("Unknown metric type: {Type}", msg.MetricType);
                break;
        }
    }
}