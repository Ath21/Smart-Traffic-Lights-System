using System;
using LogMessages;
using LogStore.Business;
using LogStore.Models.Dtos;
using MassTransit;

namespace LogStore.Consumers.Traffic;

/*  Queue: log.traffic_layer.audit.queue
    Exchange: LOG.EXCHANGE (topic)
    Bindings:
        - log.traffic.analytics_service.audit
        - log.traffic.coordinator_service.audit
        - log.traffic.intersection_controller_service.audit
        - log.traffic.light_controller_service.audit
*/
public class TrafficAuditLogConsumer : IConsumer<AuditLogMessage>
{
    private readonly ILogService _logService;
    private readonly ILogger<TrafficAuditLogConsumer> _logger;
    private const string ServiceTag = "[" + nameof(TrafficAuditLogConsumer) + "]";

    public TrafficAuditLogConsumer(ILogService logService, ILogger<TrafficAuditLogConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuditLogMessage> context)
    {
        var msg = context.Message;
        var log = new AuditLogDto
        {
            LogId = msg.LogId,
            ServiceName = msg.ServiceName,
            Action = msg.Action,
            Message = msg.Details,
            Timestamp = msg.Timestamp,
            Metadata = msg.Metadata as Dictionary<string, object>
        };

        _logger.LogInformation("{Tag} [Audit] {Action} by {Service}",
            ServiceTag, log.Action, log.ServiceName);

        await _logService.StoreAuditLogAsync(log);
    }
}
