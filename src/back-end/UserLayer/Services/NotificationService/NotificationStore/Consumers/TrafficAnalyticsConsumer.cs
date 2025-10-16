using System;
using MassTransit;
using Messages.Traffic;
using NotificationStore.Business;
using NotificationStore.Business.MessageHandler;

namespace NotificationStore.Consumers;

public class TrafficAnalyticsConsumer : IConsumer<TrafficAnalyticsMessage>
{
    private readonly INotificationProcessor _processor;
    private readonly ILogger<TrafficAnalyticsConsumer> _logger;

    public TrafficAnalyticsConsumer(INotificationProcessor processor, ILogger<TrafficAnalyticsConsumer> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficAnalyticsMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("[CONSUMER][TRAFFIC_ANALYTICS] {MetricType} metric received for {Intersection}",
            msg.MetricType, msg.IntersectionName);

        try
        {
            await _processor.HandleTrafficAnalyticsAsync(msg);
            _logger.LogInformation("Processed {MetricType} for {Intersection}", msg.MetricType, msg.IntersectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling traffic analytics message for {Intersection}", msg.IntersectionName);
        }
    }
}
