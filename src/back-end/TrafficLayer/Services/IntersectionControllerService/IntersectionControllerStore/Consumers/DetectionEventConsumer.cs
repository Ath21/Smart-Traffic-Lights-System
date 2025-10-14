using MassTransit;
using Microsoft.Extensions.Logging;
using Messages.Sensor;

namespace IntersectionControllerStore.Consumers;

public class DetectionEventConsumer : IConsumer<DetectionEventMessage>
{
    private readonly ILogger<DetectionEventConsumer> _logger;

    public DetectionEventConsumer(ILogger<DetectionEventConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DetectionEventMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER][EVENT][{Intersection}] {EventType} detected ({VehicleType}, Dir={Direction})",
            msg.IntersectionName,
            msg.EventType,
            msg.VehicleType,
            msg.Direction);

        switch (msg.EventType.ToLowerInvariant())
        {
            case "emergency vehicle":
                _logger.LogInformation("→ Initiating emergency vehicle handling logic for {Intersection}", msg.IntersectionName);
                break;

            case "public transport":
                _logger.LogInformation("→ Applying priority timing for public transport at {Intersection}", msg.IntersectionName);
                break;

            case "incident":
                _logger.LogWarning("→ Incident detected at {Intersection}, notifying analytics/log subsystems", msg.IntersectionName);
                break;
        }

        await Task.CompletedTask;
    }
}
