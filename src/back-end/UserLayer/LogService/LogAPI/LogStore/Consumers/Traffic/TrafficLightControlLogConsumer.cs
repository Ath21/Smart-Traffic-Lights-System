using System;
using LogStore.Business;
using LogStore.Models;
using MassTransit;
using TrafficMessages;

namespace LogStore.Consumers.Traffic;

public class TrafficLightControlLogConsumer : IConsumer<TrafficLightControlLog>
{
    private readonly ILogService _logService;

    public TrafficLightControlLogConsumer(ILogService logService)
    {
        _logService = logService;
    }

    public async Task Consume(ConsumeContext<TrafficLightControlLog> context)
    {
        var msg = context.Message;

        var log = new LogDto
        {
            LogLevel = "INFO",
            Service = "Traffic Light Control Service",
            Message = $"[Control] Intersection {msg.IntersectionId} - New Pattern: {msg.ControlPattern}, Duration: {msg.DurationSeconds}s",
            Timestamp = msg.Timestamp
        };

        await _logService.StoreLogAsync(log);
    }
}
