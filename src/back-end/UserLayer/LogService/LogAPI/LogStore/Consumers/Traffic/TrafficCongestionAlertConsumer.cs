using System;
using LogStore.Business;
using LogStore.Messages.Traffic;
using LogStore.Models;
using MassTransit;

namespace LogStore.Consumers.Traffic;

public class TrafficCongestionAlertConsumer : IConsumer<TrafficCongestionAlert>
{
    private readonly ILogService _logService;

    public TrafficCongestionAlertConsumer(ILogService logService)
    {
        _logService = logService;
    }

    public async Task Consume(ConsumeContext<TrafficCongestionAlert> context)
    {
        var msg = context.Message;

        var log = new LogDto
        {
            LogLevel = "WARNING",
            Service = "Traffic Data Analytics Service",
            Message = $"[ALERT] Congestion at {msg.IntersectionId} - Level: {msg.CongestionLevel:P0}, Severity: {msg.Severity}. {msg.Description}",
            Timestamp = msg.Timestamp
        };

        await _logService.StoreLogAsync(log);
    }
}
