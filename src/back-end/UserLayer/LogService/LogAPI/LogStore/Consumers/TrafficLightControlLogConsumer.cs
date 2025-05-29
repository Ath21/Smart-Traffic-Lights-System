using System;
using LogStore.Business;
using LogStore.Messages;
using LogStore.Models;
using MassTransit;

namespace LogStore.Consumers;

public class TrafficLightControlLogConsumer : IConsumer<TrafficLogMessage>
{
    private readonly ILogService _logService;

    public TrafficLightControlLogConsumer(ILogService logService)
    {
        _logService = logService;
    }

    public async Task Consume(ConsumeContext<TrafficLogMessage> context)
    {
        var message = context.Message;
        var logDto = new LogDto
        {
            LogLevel = "Info",
            Service = "Traffic Light Control Service",
            Message = message.Message,
            TraceId = context.MessageId?.ToString() ?? Guid.NewGuid().ToString(),
            Timestamp = message.Timestamp
        };

        await _logService.StoreLogAsync(logDto);
    }
}
