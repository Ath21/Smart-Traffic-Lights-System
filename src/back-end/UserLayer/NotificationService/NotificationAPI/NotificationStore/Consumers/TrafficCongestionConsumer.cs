using MassTransit;
using NotificationStore.Business.Notify;
using NotificationStore.Models;
using TrafficMessages;

namespace NotificationStore.Consumers;

public class TrafficCongestionConsumer : IConsumer<TrafficCongestionMessage>
{
    private readonly ILogger<TrafficCongestionConsumer> _logger;
    private readonly INotificationService _notificationService;

    public TrafficCongestionConsumer(ILogger<TrafficCongestionConsumer> logger, INotificationService notificationService)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficCongestionMessage> context)
    {
        var dto = new NotificationDto
        {
            Type = "TrafficCongestion",
            Title = $"Congestion at Intersection {context.Message.IntersectionId}",
            Message = context.Message.Message,
            TargetAudience = "Drivers",
            CreatedAt = context.Message.Timestamp,
            Status = "Pending"
        };

        _logger.LogInformation(
            "TrafficCongestionConsumer: Congestion alert at intersection {IntersectionId} with level {Level}",
            context.Message.IntersectionId, context.Message.CongestionLevel);

        await _notificationService.SendNotificationAsync(dto);
    }
}
