using System;
using MassTransit;
using Messages.Traffic;

namespace TrafficLightCoordinatorStore.Consumers;

public class TrafficAnalyticsConsumer : IConsumer<TrafficAnalyticsMessage>
{
    private readonly ILogger<TrafficAnalyticsConsumer> _logger;

    public TrafficAnalyticsConsumer(ILogger<TrafficAnalyticsConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficAnalyticsMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("[COORDINATOR][ANALYTICS] {Metric} received for {Intersection} (Severity={Severity})",
            msg.MetricType, msg.IntersectionName, msg.Severity);

        // Simplified decision logic
        if (msg.MetricType == "Congestion" && msg.Severity >= 3)
        {
            var mode = new { Red = 20, Orange = 5, Green = 50, Offset = 10 };
            _logger.LogWarning("→ Applying CONGESTION mode to {Intersection}: {@Mode}", msg.IntersectionName, mode);
            // TODO: send to Intersection Controller via priority queue: traffic.light.control.{intersection}.light
        }
        else if (msg.MetricType == "Incident")
        {
            var mode = new { Red = 30, Orange = 5, Green = 40, Offset = 15 };
            _logger.LogError("→ Applying INCIDENT mode to {Intersection}: {@Mode}", msg.IntersectionName, mode);
        }

        await Task.CompletedTask;
    }
}
