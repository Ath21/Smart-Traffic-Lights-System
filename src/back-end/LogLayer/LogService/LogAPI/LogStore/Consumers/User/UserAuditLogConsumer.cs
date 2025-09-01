using LogMessages;
using LogStore.Business;
using LogStore.Models.Dtos;
using MassTransit;

namespace LogStore.Consumers.User;

// Queue: user.log_service.queue → log.user.<service_name>.audit
public class UserAuditLogConsumer : IConsumer<AuditLogMessage>
{
    private readonly ILogService _logService;
    private readonly ILogger<UserAuditLogConsumer> _logger;
    private const string ServiceTag = "[" + nameof(UserAuditLogConsumer) + "]";

    public UserAuditLogConsumer(ILogService logService, ILogger<UserAuditLogConsumer> logger)
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

        _logger.LogInformation("{Tag} [Audit] {Action} by {Service} at {Timestamp}",
            ServiceTag, log.Action, log.ServiceName, log.Timestamp);

        await _logService.StoreAuditLogAsync(log);
    }
}