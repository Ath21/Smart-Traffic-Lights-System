/* 
 * UserStore.Publishers.UserLogPublisher
 * 
 * This class implements the IUserLogPublisher interface to publish user-related logs
 * such as informational logs, audit logs, and error logs to a message broker (e.g., RabbitMQ).
 * It uses MassTransit for message publishing and is typically used in the UserService layer of the application.
 * It is part of the UserStore project, which is responsible for managing user-related operations
 * and services.
 * 
 * Dependencies:
 *   - ISendEndpointProvider: Interface for sending messages to a specific endpoint.
 *   - IConfiguration: Interface for accessing application configuration settings.
 *   - ILogger<UserLogPublisher>: Interface for logging messages.
 *   - IBus: Interface for interacting with the message bus.
 * Methods:
 *   - Task PublishInfoAsync(string message): Publishes an informational log message.
 *   - Task PublishAuditAsync(Guid userId, string action, string details): Publishes an audit log message
 *     for a specific user action.
 *   - Task PublishErrorAsync(string message, Exception exception): Publishes an error log message
 *     along with an exception.
 *   - Task PublishNotificationAsync(Guid recipientId, string recipientEmail, string message, string type):
 */
using MassTransit;
using UserMessages;

namespace UserStore.Publishers;

public class UserLogPublisher : IUserLogPublisher
{
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly IConfiguration _configuration;
    private readonly IBus _bus;
    private readonly ILogger<UserLogPublisher> _logger;

    private readonly string _logsExchangeName;
    private readonly string _notificationsExchangeName;
    private readonly string _infoKey;
    private readonly string _auditKey;
    private readonly string _errorKey;
    private readonly string _notificationKey;

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

        _logsExchangeName = section["UserLogsExchange"];
        _infoKey = section["RoutingKeys:Info"];
        _auditKey = section["RoutingKeys:Audit"];
        _errorKey = section["RoutingKeys:Error"];

        _notificationsExchangeName = section["UserNotificationsExchange"];
        _notificationKey = section["RoutingKeys:NotificationsRequest"];
    }

    public async Task PublishAuditAsync(Guid userId, string action, string details)
    {
        _logger.LogInformation("[AUDIT] Preparing to publish audit log to exchange '{Exchange}' with routing key '{RoutingKey}'", _logsExchangeName, _auditKey);

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
        _logger.LogInformation("[ERROR] Preparing to publish error log to exchange '{Exchange}' with routing key '{RoutingKey}'", _logsExchangeName, _errorKey);

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
        _logger.LogInformation("[INFO] Preparing to publish info log to exchange '{Exchange}' with routing key '{RoutingKey}'", _logsExchangeName, _infoKey);

        await _bus.Publish(new LogInfo(
            message,
            DateTime.UtcNow), context =>
        {
            context.SetRoutingKey(_infoKey);
        });

        _logger.LogInformation("[INFO] Info log published: {Message}", message);
    }

    public async Task PublishNotificationAsync(Guid recipientId, string recipientEmail, string message, string type)
    {
        _logger.LogInformation("[NOTIFICATION] Preparing to publish notification request to exchange '{Exchange}' with routing key '{RoutingKey}'", _notificationsExchangeName, _notificationKey);

        await _bus.Publish(new NotificationRequest(
            recipientId,
            recipientEmail,
            message,
            type,
            DateTime.UtcNow), context =>
        {
            context.SetRoutingKey(_notificationKey);
        });

        _logger.LogInformation("[NOTIFICATION] Notification request published for recipient {RecipientId}, type {Type}", recipientId, type);
    }
}
