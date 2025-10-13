using System;
using LogMessages;
using LogStore.Business;
using LogStore.Models.Dtos;
using MassTransit;

namespace LogStore.Consumers.User;

/*  Queue: log.user_layer.error.queue
    Exchange: LOG.EXCHANGE (topic)
    Bindings:
        - log.user.user_service.error
        - log.user.notification_service.error
*/
public class UserErrorLogConsumer : IConsumer<ErrorLogMessage>
{
    private readonly ILogService _logService;
    private readonly ILogger<UserErrorLogConsumer> _logger;
    private const string ServiceTag = "[" + nameof(UserErrorLogConsumer) + "]";

    public UserErrorLogConsumer(ILogService logService, ILogger<UserErrorLogConsumer> logger)
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
