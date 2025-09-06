using System;
using LogMessages;
using LogStore.Business;
using LogStore.Models.Dtos;
using MassTransit;

namespace LogStore.Consumers.Sensor;

// Queue: sensor.log_service.queue → log.sensor.<service_name>.audit
public class SensorAuditLogConsumer : IConsumer<AuditLogMessage>
{
    private readonly ILogService _logService;
    private readonly ILogger<SensorAuditLogConsumer> _logger;
    private const string ServiceTag = "[" + nameof(SensorAuditLogConsumer) + "]";

    public SensorAuditLogConsumer(ILogService logService, ILogger<SensorAuditLogConsumer> logger)
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
