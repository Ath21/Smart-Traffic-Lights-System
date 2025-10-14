using System;
using MassTransit;
using Messages.Traffic;
using TrafficLightCoordinatorStore.Business;

namespace TrafficLightCoordinatorStore.Consumers;

public class TrafficAnalyticsConsumer : IConsumer<TrafficAnalyticsMessage>
{
    private readonly ICoordinatorBusiness _business;
    private readonly ILogger<TrafficAnalyticsConsumer> _logger;

    public TrafficAnalyticsConsumer(ICoordinatorBusiness business, ILogger<TrafficAnalyticsConsumer> logger)
    {
        _business = business;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficAnalyticsMessage> context)
    {
        _logger.LogInformation("[CONSUMER] TrafficAnalyticsMessage received ({Metric})", context.Message.MetricType);
        await _business.HandleTrafficAnalyticsAsync(context.Message);
    }
}