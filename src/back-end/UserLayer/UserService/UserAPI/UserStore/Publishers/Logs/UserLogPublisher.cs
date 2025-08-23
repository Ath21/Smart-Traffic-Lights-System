using MassTransit;
using LogMessages;

namespace UserStore.Publishers.Logs;

public class UserLogPublisher : IUserLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<UserLogPublisher> _logger;
    private readonly string _serviceName = "user_service";
    private readonly string _auditKey;
    private readonly string _errorKey;

    public UserLogPublisher(IConfiguration configuration, ILogger<UserLogPublisher> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;

        var section = configuration.GetSection("RabbitMQ:RoutingKeys");
        _auditKey = section["Audit"] ?? "log.user.user_service.audit";
        _errorKey = section["Error"] ?? "log.user.user_service.error";
    }

    public async Task PublishAuditAsync(string action, string details, object? metadata = null)
    {
        var message = new AuditLogMessage(
            Guid.NewGuid(),
            _serviceName,
            action,
            details,
            DateTime.UtcNow,
            metadata
        );

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(_auditKey));

        _logger.LogInformation("Audit log published: {Action}", action);
    }

    public async Task PublishErrorAsync(string errorType, string messageText, object? metadata = null)
    {
        var message = new ErrorLogMessage(
            Guid.NewGuid(),
            _serviceName,
            errorType,
            messageText,
            DateTime.UtcNow,
            metadata
        );

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(_errorKey));

        _logger.LogError("Error log published: {ErrorType} - {Message}", errorType, messageText);
    }
}
