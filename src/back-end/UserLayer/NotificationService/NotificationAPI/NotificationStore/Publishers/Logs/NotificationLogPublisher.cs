using LogMessages;
using MassTransit;

namespace NotificationStore.Publishers.Logs;

public class NotificationLogPublisher : INotificationLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<NotificationLogPublisher> _logger;
    private readonly string _serviceName;
    private readonly string _auditKey;
    private readonly string _errorKey;

    private const string ServiceTag = "[" + nameof(NotificationLogPublisher) + "]";

    public NotificationLogPublisher(IConfiguration configuration, ILogger<NotificationLogPublisher> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;

        _serviceName = "notification_service";

        _auditKey = configuration["RabbitMQ:RoutingKeys:Audit"] 
                    ?? "log.user.notification_service.audit";
        _errorKey = configuration["RabbitMQ:RoutingKeys:Error"] 
                    ?? "log.user.notification_service.error";
    }

    // log.user.notification_service.audit
    public async Task PublishAuditLogAsync(string action, string details, object? metadata = null)
    {
        var log = new AuditLogMessage(
            Guid.NewGuid(),
            _serviceName,
            action,
            details,
            DateTime.UtcNow,
            metadata
        );

        await _bus.Publish(log, ctx => ctx.SetRoutingKey(_auditKey));

        _logger.LogInformation("{Tag} Audit log published: {Action}", ServiceTag, action);
    }

    // log.user.notification_service.error
    public async Task PublishErrorLogAsync(string errorType, string message, object? metadata = null, Exception? ex = null)
    {
        var log = new ErrorLogMessage(
            Guid.NewGuid(),
            _serviceName,
            errorType,
            message,
            DateTime.UtcNow,
            metadata
        );

        await _bus.Publish(log, ctx => ctx.SetRoutingKey(_errorKey));

        _logger.LogError(ex, "{Tag} Error log published: {ErrorType} - {Message}", ServiceTag, errorType, message);
    }
}
