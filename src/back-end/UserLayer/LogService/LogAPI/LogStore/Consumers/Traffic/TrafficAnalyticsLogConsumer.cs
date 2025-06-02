using System;
using LogStore.Business;
using LogStore.Messages.Traffic;
using LogStore.Models;
using MassTransit;

namespace LogStore.Consumers.Traffic;

public class TrafficAnalyticsLogConsumer : IConsumer<TrafficDailySummary>
{
    private readonly ILogService _logService;

    public TrafficAnalyticsLogConsumer(ILogService logService)
    {
        _logService = logService;
    }

    public async Task Consume(ConsumeContext<TrafficDailySummary> context)
    {
        var msg = context.Message;

        var log = new LogDto
        {
            LogLevel = "INFO",
            Service = "TrafficService",
            Message = $"Daily Summary for {msg.IntersectionId}: AvgSpeed={msg.AverageSpeed} km/h, VehicleCount={msg.VehicleCount}",
            Timestamp = msg.Timestamp
        };

        await _logService.StoreLogAsync(log);
    }
}