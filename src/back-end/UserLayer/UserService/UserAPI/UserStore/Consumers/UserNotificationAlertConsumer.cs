using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserMessages;

namespace UserStore.Consumers;

public class UserNotificationAlertConsumer : IConsumer<UserNotificationAlert>
{
    private readonly ILogger<UserNotificationAlertConsumer> _logger;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public UserNotificationAlertConsumer(
        ILogger<UserNotificationAlertConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _queueName = configuration["RabbitMQ:Queues:UserNotificationAlertQueue"] ?? "user.notification.alert.queue";
        _exchangeName = configuration["RabbitMQ:Exchanges:UserExchange"] ?? "user.exchange";
        _routingKeyPattern = configuration["RabbitMQ:RoutingKeys:UserNotificationAlert"] ?? "user.notification.alert";
    }

    public Task Consume(ConsumeContext<UserNotificationAlert> context)
    {
        var msg = context.Message;

        _logger.LogInformation("Received UserNotificationAlert for User {UserId}, Email {Email}, Type {AlertType}, Message {Message}",
            msg.UserId, msg.Email, msg.AlertType, msg.Message);

        // TODO: handle notification persistence or forward to front-end signal

        return Task.CompletedTask;
    }
}
