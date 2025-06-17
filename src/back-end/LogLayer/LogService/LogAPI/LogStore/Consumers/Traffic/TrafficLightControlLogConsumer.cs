/*
 *  LogStore.Consumers.Traffic.TrafficLightControlLogConsumer
 *
 *  This class implements the IConsumer interface for handling TrafficLightControlLog messages.
 *  It consumes messages related to traffic light control and logs the control actions.
 *  The consumer uses the ILogService to store logs in the database.
 *  The message contains information about the intersection ID, control pattern, duration, and timestamp.
 *  The log message is formatted and stored in the database for later retrieval and analysis.
 */
using LogStore.Business;
using LogStore.Models;
using MassTransit;
using TrafficMessages;

namespace LogStore.Consumers.Traffic;

public class TrafficLightControlLogConsumer : IConsumer<TrafficLightControl>
{
    private readonly ILogService _logService;
    private readonly ILogger<TrafficLightControlLogConsumer> _logger;

    public TrafficLightControlLogConsumer(ILogService logService, ILogger<TrafficLightControlLogConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TrafficLightControl> context)
    {
        var msg = context.Message;

        var log = new LogDto
        {
            LogLevel = "INFO",
            Service = "Traffic Light Control Service",
            Message = $"[Control] Intersection {msg.IntersectionId} - New Pattern: {msg.ControlPattern}, Duration: {msg.DurationSeconds}s",
            Timestamp = msg.Timestamp
        };

        _logger.LogInformation("TrafficLightControlConsumer: {Message} at {Timestamp}", log.Message, log.Timestamp);

        await _logService.StoreLogAsync(log);
    }
}
