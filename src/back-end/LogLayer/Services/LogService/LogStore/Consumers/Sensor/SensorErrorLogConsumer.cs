using System;
using LogMessages;
using LogStore.Business;
using LogStore.Models.Dtos;
using MassTransit;

namespace LogStore.Consumers.Sensor;

// Queue: sensor.log_service.queue → log.sensor.<service_name>.error
public class SensorErrorLogConsumer : IConsumer<ErrorLogMessage>
{
    private readonly ILogService _logService;
    private readonly ILogger<SensorErrorLogConsumer> _logger;
    private const string ServiceTag = "[" + nameof(SensorErrorLogConsumer) + "]";

    public SensorErrorLogConsumer(ILogService logService, ILogger<SensorErrorLogConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ErrorLogMessage> context)
    {
        var msg = context.Message;
        var log = new ErrorLogDto
        {
            LogId = msg.LogId,
            ServiceName = msg.ServiceName,
            ErrorType = msg.ErrorType,
            Message = msg.Message,
            Timestamp = msg.Timestamp,
            Metadata = msg.Metadata as Dictionary<string, object>
        };

        _logger.LogError("{Tag} [Error] {ErrorType} in {Service}: {Message}",
            ServiceTag, log.ErrorType, log.ServiceName, log.Message);

        await _logService.StoreErrorLogAsync(log);
    }
}
