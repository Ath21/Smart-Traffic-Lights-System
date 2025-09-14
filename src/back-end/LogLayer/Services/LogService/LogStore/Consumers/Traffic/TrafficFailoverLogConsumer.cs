using System;
using LogMessages;
using LogStore.Business;
using LogStore.Models.Dtos;
using MassTransit;

namespace LogStore.Consumers.Traffic;

/*  Queue: log.traffic_layer.failover.queue
    Exchange: LOG.EXCHANGE (topic)
    Bindings:
        - log.traffic.analytics_service.{intersection}.failover
        - log.traffic.coordinator_service.{intersection}.failover
        - log.traffic.intersection_controller_service.{intersection}.failover
        - log.traffic.light_controller_service.{intersection}.{light}.failover
*/
public class TrafficFailoverLogConsumer : IConsumer<FailoverMessage>
{
    private readonly ILogService _logService;
    private readonly ILogger<TrafficFailoverLogConsumer> _logger;
    private const string ServiceTag = "[" + nameof(TrafficFailoverLogConsumer) + "]";

    public TrafficFailoverLogConsumer(ILogService logService, ILogger<TrafficFailoverLogConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FailoverMessage> context)
    {
        var msg = context.Message;
        var log = new FailoverLogDto
        {
            LogId = msg.LogId,
            ServiceName = msg.ServiceName,
            Context = msg.Context,
            Reason = msg.Reason,
            Mode = msg.Mode,
            Timestamp = msg.Timestamp,
            Metadata = msg.Metadata as Dictionary<string, object>
        };

        _logger.LogWarning("{Tag} [Failover] {Context} in {Service} -> Mode={Mode}, Reason={Reason}",
            ServiceTag, log.Context, log.ServiceName, log.Mode, log.Reason);

        await _logService.StoreFailoverLogAsync(log);
    }
}
