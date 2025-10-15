using MassTransit;
using Microsoft.Extensions.Logging;
using Messages.Sensor;
using IntersectionControllerStore.Business.Priority;

namespace IntersectionControllerStore.Consumers;


public class DetectionEventConsumer : IConsumer<DetectionEventMessage>
{
    private readonly IPriorityBusiness _priorityProcessor;
    private readonly ILogger<DetectionEventConsumer> _logger;

    public DetectionEventConsumer(IPriorityBusiness priorityProcessor, ILogger<DetectionEventConsumer> logger)
    {
        _priorityProcessor = priorityProcessor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DetectionEventMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER][DETECTION][{Intersection}] EventType={Event}, Vehicle={VehicleType}, Direction={Direction}",
            msg.IntersectionName, msg.EventType, msg.VehicleType, msg.Direction);

        try
        {
            await _priorityProcessor.HandleDetectionEventAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CONSUMER][DETECTION][{Intersection}] Error processing detection", msg.IntersectionName);
            throw;
        }
    }
}
