using System;
using IntersectionControllerStore.Business.LightSchedule;
using MassTransit;
using Messages.Traffic;

namespace IntersectionControllerStore.Consumers;

public class LightScheduleConsumer : IConsumer<TrafficLightScheduleMessage>
{
    private readonly LightScheduleBusiness _processor;
    private readonly ILogger<LightScheduleConsumer> _logger;

    public LightScheduleConsumer(LightScheduleBusiness processor, ILogger<LightScheduleConsumer> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficLightScheduleMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[CONSUMER][SCHEDULE][{Intersection}] Received update: Mode={Mode}, Cycle={Cycle}s, Offset={Offset}s, Purpose={Purpose}",
            msg.IntersectionName, msg.CurrentMode, msg.CycleDurationSec, msg.GlobalOffsetSec, msg.Purpose);

        try
        {
            await _processor.ProcessScheduleAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CONSUMER][SCHEDULE][{Intersection}] Failed to apply schedule", msg.IntersectionName);
            throw;
        }
    }
}