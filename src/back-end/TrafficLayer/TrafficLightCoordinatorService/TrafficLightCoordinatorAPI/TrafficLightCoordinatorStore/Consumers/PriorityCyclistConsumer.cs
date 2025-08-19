using System;
using MassTransit;
using TrafficLightCoordinatorStore.Publishers.Update;
using TrafficMessages.Priority;

namespace TrafficLightCoordinatorStore.Consumers;

public class PriorityCyclistConsumer : IConsumer<PriorityCyclist>
{
    private readonly ILightUpdatePublisher _publisher;
    private readonly ILogger<PriorityCyclistConsumer> _logger;

    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public PriorityCyclistConsumer(
        ILightUpdatePublisher publisher,
        ILogger<PriorityCyclistConsumer> logger,
        IConfiguration configuration)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var section = configuration.GetSection("RabbitMQ");
        _queueName = section["Queue:TrafficCoordinationQueue"]     ?? "traffic.light.coordination";
        _exchangeName = section["Exchange:PriorityCyclist"]        ?? "priority.cyclist";
        _routingKeyPattern = section["RoutingKey:PriorityCyclistKey"] ?? "*";
    }

    public async Task Consume(ConsumeContext<PriorityCyclist> context)
    {
        var msg = context.Message;
        _logger.LogInformation("[PRIORITY] Cyclist received on '{Exchange}' key '{Key}' -> intersection {Id}, active={Active}, ts={Ts}",
            _exchangeName, _routingKeyPattern, msg.IntersectionId, msg.PriorityActive, msg.Timestamp);

        var pattern = PatternBuilder.For("cyclist", msg.PriorityActive);
        await _publisher.PublishAsync(msg.IntersectionId, pattern, context.CancellationToken);
        _logger.LogInformation("[PRIORITY] Cyclist applied -> {Pattern}", pattern);
    }
}
