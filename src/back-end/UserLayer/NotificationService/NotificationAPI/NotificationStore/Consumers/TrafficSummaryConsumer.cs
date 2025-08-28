using MassTransit;
using NotificationStore.Business.Notify;
using NotificationStore.Models.Dtos;
using TrafficMessages;

namespace NotificationStore.Consumers;

public class TrafficSummaryConsumer : IConsumer<TrafficSummaryMessage>
{
    private readonly ILogger<TrafficSummaryConsumer> _logger;
    private readonly INotificationService _notificationService;

    private const string ServiceTag = "[" + nameof(TrafficSummaryConsumer) + "]";

    public TrafficSummaryConsumer(ILogger<TrafficSummaryConsumer> logger, INotificationService notificationService)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficSummaryMessage> context)
    {
        var dto = new NotificationDto
        {
            Type = "TrafficSummary",
            Title = $"Traffic Summary for Intersection {context.Message.IntersectionId}",
            Message = $"Avg Speed: {context.Message.AvgSpeed} | Vehicles: {context.Message.VehicleCount} | Congestion: {context.Message.CongestionLevel}",
            TargetAudience = "TrafficManagement",
            CreatedAt = context.Message.Date,
            Status = "Pending"
        };

        _logger.LogInformation(
            "{Tag} Summary received for intersection {IntersectionId} at {Date}",
            ServiceTag, context.Message.IntersectionId, context.Message.Date);

        await _notificationService.SendNotificationAsync(dto);
    }
}
