using System;
using MassTransit;
using NotificationStore.Business.Notify;
using NotificationStore.Models;
using TrafficMessages;

namespace NotificationStore.Consumers;

public class TrafficDailySummaryConsumer : IConsumer<TrafficDailySummary>
{
    private readonly INotificationService _notificationService;

    public TrafficDailySummaryConsumer(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<TrafficDailySummary> context)
    {
        var summary = context.Message;

        var dto = new NotificationDto
        {
            Type = "SUMMARY",
            RecipientId = Guid.Parse("00000000-0000-0000-0000-000000000001"), // example ID
            RecipientEmail = "admin@system.local",
            Message = $"📊 Daily summary for {summary.IntersectionId}: {summary.VehicleCount} vehicles, Avg Speed: {summary.AverageSpeed} km/h",
            Timestamp = summary.Timestamp
        };

        await _notificationService.CreateAsync(dto);
        await _notificationService.SendNotificationAsync(dto);
    }
}
