using MassTransit;
using Microsoft.Extensions.Logging;
using Messages.Sensor;
using IntersectionControllerStore.Business.Priority;

namespace IntersectionControllerStore.Consumers;

public class SensorCountConsumer : IConsumer<SensorCountMessage>
{
    private readonly PriorityBusiness _priorityProcessor;
    private readonly ILogger<SensorCountConsumer> _logger;

    public SensorCountConsumer(PriorityBusiness priorityProcessor, ILogger<SensorCountConsumer> logger)
    {
        _priorityProcessor = priorityProcessor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SensorCountMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER][COUNT][{Intersection}] {Type}={Count}, Speed={Speed:F1} km/h, Wait={Wait:F1}s, Flow={Flow:F2}/s",
            msg.IntersectionName, msg.CountType, msg.Count, msg.AverageSpeedKmh, msg.AverageWaitTimeSec, msg.FlowRate);

        try
        {
            await _priorityProcessor.HandleSensorCountAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CONSUMER][COUNT][{Intersection}] Error processing count", msg.IntersectionName);
            throw;
        }
    }
}
