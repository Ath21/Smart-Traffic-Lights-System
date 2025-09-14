using System;
using LogMessages;
using LogStore.Business;
using LogStore.Models.Dtos;
using MassTransit;

namespace LogStore.Consumers.User;

/*  Queue: log.user_layer.failover.queue
    Exchange: LOG.EXCHANGE (topic)
    Bindings:
        - log.user.user_service.failover
        - log.user.notification_service.failover
*/
public class UserFailoverLogConsumer : IConsumer<FailoverMessage>
{
    private readonly ILogService _logService;
    private readonly ILogger<UserFailoverLogConsumer> _logger;
    private const string ServiceTag = "[" + nameof(UserFailoverLogConsumer) + "]";

    public UserFailoverLogConsumer(ILogService logService, ILogger<UserFailoverLogConsumer> logger)
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

        _logger.LogWarning(
            "{Tag} [Failover] {Context} in {Service} → Mode={Mode}, Reason={Reason}, Timestamp={Timestamp}",
            ServiceTag, log.Context, log.ServiceName, log.Mode, log.Reason, log.Timestamp);

        await _logService.StoreFailoverLogAsync(log);
    }
}
