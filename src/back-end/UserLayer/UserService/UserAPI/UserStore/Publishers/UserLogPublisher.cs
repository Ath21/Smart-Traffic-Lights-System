using System;
using MassTransit;
using UserMessages;

namespace UserStore.Publishers;

public class UserLogPublisher : IUserLogPublisher
{
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly IConfiguration _configuration;
    private readonly IBus _bus;
    private readonly ILogger<UserLogPublisher> _logger;

    private readonly string _exchangeName;
    private readonly string _infoKey;
    private readonly string _auditKey;
    private readonly string _errorKey;

    public UserLogPublisher(
        ISendEndpointProvider sendEndpointProvider,
        IConfiguration configuration,
        ILogger<UserLogPublisher> logger,
        IBus bus)
    {
        _sendEndpointProvider = sendEndpointProvider;
        _configuration = configuration;
        _logger = logger;
        _bus = bus;

        var section = _configuration.GetSection("RabbitMQ");
        _exchangeName = section["UserLogsExchange"];
        _infoKey = section["RoutingKeys:Info"];
        _auditKey = section["RoutingKeys:Audit"];
        _errorKey = section["RoutingKeys:Error"];
    }

    public async Task PublishAuditAsync(Guid userId, string action, string details)
    {
        _logger.LogInformation("[AUDIT] Preparing to publish audit log to exchange '{Exchange}' with routing key '{RoutingKey}'", _exchangeName, _auditKey);

        await _bus.Publish(new LogAudit(
            userId,
            action,
            details,
            DateTime.UtcNow), context =>
        {
            context.SetRoutingKey(_auditKey);
        });

        _logger.LogInformation("[AUDIT] Audit log published for user {UserId}, action {Action}", userId, action);
    }

    public async Task PublishErrorAsync(string message, Exception exception)
    {
        _logger.LogInformation("[ERROR] Preparing to publish error log to exchange '{Exchange}' with routing key '{RoutingKey}'", _exchangeName, _errorKey);

        await _bus.Publish(new LogError(
            message,
            exception.ToString(),
            DateTime.UtcNow), context =>
        {
            context.SetRoutingKey(_errorKey);
        });

        _logger.LogInformation("[ERROR] Error log published: {Message}", message);
    }

    public async Task PublishInfoAsync(string message)
    {
        _logger.LogInformation("[INFO] Preparing to publish info log to exchange '{Exchange}' with routing key '{RoutingKey}'", _exchangeName, _infoKey);

        await _bus.Publish(new LogInfo(
            message,
            DateTime.UtcNow), context =>
        {
            context.SetRoutingKey(_infoKey);
        });

        _logger.LogInformation("[INFO] Info log published: {Message}", message);
    }
}
