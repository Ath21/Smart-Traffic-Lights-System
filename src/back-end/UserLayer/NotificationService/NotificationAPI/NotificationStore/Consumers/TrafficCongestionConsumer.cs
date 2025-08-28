using MassTransit;
using NotificationStore.Business.Notify;
using NotificationStore.Models.Dtos;
using TrafficMessages;

namespace NotificationStore.Consumers;

public class TrafficCongestionConsumer : IConsumer<TrafficCongestionMessage>
{
    private readonly ILogger<TrafficCongestionConsumer> _logger;
    private readonly INotificationService _notificationService;

    private const string ServiceTag = "[" + nameof(TrafficCongestionConsumer) + "]";

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
            "{Tag} Congestion alert at intersection {IntersectionId} with level {Level}",
            ServiceTag, context.Message.IntersectionId, context.Message.CongestionLevel);

        await _notificationService.SendNotificationAsync(dto);
    }
}
