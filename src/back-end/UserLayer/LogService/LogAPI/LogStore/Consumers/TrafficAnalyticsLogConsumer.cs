using System;
using LogStore.Business;
using LogStore.Messages;
using LogStore.Models;
using MassTransit;

namespace LogStore.Consumers;

public class TrafficAnalyticsLogConsumer : IConsumer<LogMessage>
{
    private readonly ILogService _logService;

public TrafficAnalyticsLogConsumer(ILogService logService)
    {
        _logService = logService;
    }

    public async Task Consume(ConsumeContext<LogMessage> context)
    {
        var message = context.Message;
        var logDto = new LogDto
        {
            LogLevel = message.LogLevel,
            Service = message.Service,
            Message = message.Message,
            Timestamp = message.Timestamp
        };

        await _logService.StoreLogAsync(logDto);
    }
}
