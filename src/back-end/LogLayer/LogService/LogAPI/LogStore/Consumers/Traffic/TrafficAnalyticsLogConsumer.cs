/*
 *  LogStore.Consumers.Traffic.TrafficAnalyticsLogConsumer
 *
 *  This class implements the IConsumer interface for handling TrafficDailySummary messages.
 *  It consumes messages related to traffic analytics and logs the daily summary of traffic data.
 *  The consumer uses the ILogService to store logs in the database.
 *  The message contains information about the intersection ID, average speed, vehicle count, and timestamp.
 *  The log message is formatted and stored in the database for later retrieval and analysis.
 */
using LogStore.Business;
using LogStore.Models;
using MassTransit;
using TrafficMessages;

namespace LogStore.Consumers.Traffic;

public class TrafficAnalyticsLogConsumer : IConsumer<TrafficDailySummary>
{
    private readonly ILogService _logService;
    private readonly ILogger<TrafficAnalyticsLogConsumer> _logger;

    public TrafficAnalyticsLogConsumer(ILogService logService, ILogger<TrafficAnalyticsLogConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficDailySummary> context)
    {
        var msg = context.Message;

        var log = new LogDto
        {
            LogLevel = "INFO",
            Service = "Traffic Service",
            Message = $"Daily Summary for {msg.IntersectionId}: AvgSpeed={msg.AverageSpeed} km/h, VehicleCount={msg.VehicleCount}",
            Timestamp = msg.Timestamp
        };

        _logger.LogInformation("TrafficAnalyticsLogConsumer: {Message} at {Timestamp}", log.Message, log.Timestamp);

        await _logService.StoreLogAsync(log);
    }
}