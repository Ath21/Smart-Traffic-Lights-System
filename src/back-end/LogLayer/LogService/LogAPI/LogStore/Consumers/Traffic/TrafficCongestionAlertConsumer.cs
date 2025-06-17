/*
 *  LogStore.Consumers.Traffic.TrafficCongestionAlertConsumer
 *
 *  This class implements the IConsumer interface for handling TrafficCongestionAlert messages.
 *  It consumes messages related to traffic congestion alerts and logs the alert details.
 *  The consumer uses the ILogService to store logs in the database.
 *  The message contains information about the intersection ID, congestion level, severity, description, and timestamp.
 *  The log message is formatted and stored in the database for later retrieval and analysis.
 */
using LogStore.Business;
using LogStore.Models;
using MassTransit;
using TrafficMessages;

namespace LogStore.Consumers.Traffic;

public class TrafficCongestionAlertConsumer : IConsumer<TrafficCongestionAlert>
{
    private readonly ILogService _logService;
    private readonly ILogger<TrafficCongestionAlertConsumer> _logger;

    public TrafficCongestionAlertConsumer(ILogService logService, ILogger<TrafficCongestionAlertConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
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

        _logger.LogWarning("TrafficCongestionAlertConsumer: {Message} at {Timestamp}", log.Message, log.Timestamp);

        await _logService.StoreLogAsync(log);
    }
}
