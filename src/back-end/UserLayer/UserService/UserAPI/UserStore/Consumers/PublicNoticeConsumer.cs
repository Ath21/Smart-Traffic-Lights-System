using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserMessages;

namespace UserStore.Consumers;

public class PublicNoticeConsumer : IConsumer<PublicNoticeEvent>
{
    private readonly ILogger<PublicNoticeConsumer> _logger;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public PublicNoticeConsumer(
        ILogger<PublicNoticeConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _queueName = configuration["RabbitMQ:Queues:PublicNoticeQueue"] ?? "notification.public_notice.queue";
        _exchangeName = configuration["RabbitMQ:Exchanges:NotificationExchange"] ?? "notification.exchange";
        _routingKeyPattern = configuration["RabbitMQ:RoutingKeys:PublicNotice"] ?? "notification.event.public_notice";
    }

    public Task Consume(ConsumeContext<PublicNoticeEvent> context)
    {
        var msg = context.Message;

        _logger.LogInformation("Received PublicNotice: {Title} - {Message} for {Audience}",
            msg.Title, msg.Message, msg.TargetAudience);

        // TODO: store notice in DB or cache for UI display

        return Task.CompletedTask;
    }
}
