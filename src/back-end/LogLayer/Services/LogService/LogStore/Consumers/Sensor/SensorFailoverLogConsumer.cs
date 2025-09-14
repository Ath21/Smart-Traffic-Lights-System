using System;
using LogMessages;
using LogStore.Business;
using LogStore.Models.Dtos;
using MassTransit;

namespace LogStore.Consumers.Sensor;

/*  Queue: log.sensor_layer.failover.queue
    Exchange: LOG.EXCHANGE (topic)
    Bindings:
        - log.sensor.sensor_service.{intersection}.failover
        - log.sensor.detection_service.{intersection}.failover
*/
public class SensorFailoverLogConsumer : IConsumer<FailoverMessage>
{
    private readonly ILogService _logService;
    private readonly ILogger<SensorFailoverLogConsumer> _logger;
    private const string ServiceTag = "[" + nameof(SensorFailoverLogConsumer) + "]";

    public SensorFailoverLogConsumer(ILogService logService, ILogger<SensorFailoverLogConsumer> logger)
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

        _logger.LogWarning("{Tag} [Failover] {Context} -> Mode={Mode}, Reason={Reason}, Service={Service}",
            ServiceTag, log.Context, log.Mode, log.Reason, log.ServiceName);

        await _logService.StoreFailoverLogAsync(log);
    }
}
