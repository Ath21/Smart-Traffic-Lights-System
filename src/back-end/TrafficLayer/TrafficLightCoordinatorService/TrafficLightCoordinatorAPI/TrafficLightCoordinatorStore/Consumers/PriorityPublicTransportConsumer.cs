using System;
using MassTransit;
using TrafficLightCoordinatorStore.Publishers.Update;
using TrafficMessages.Priority;

namespace TrafficLightCoordinatorStore.Consumers;

public class PriorityPublicTransportConsumer : IConsumer<PriorityPublicTransport>
{
    private readonly ILightUpdatePublisher _publisher;
    private readonly ILogger<PriorityPublicTransportConsumer> _logger;

    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public PriorityPublicTransportConsumer(
        ILightUpdatePublisher publisher,
        ILogger<PriorityPublicTransportConsumer> logger,
        IConfiguration configuration)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var section = configuration.GetSection("RabbitMQ");
        _queueName = section["Queue:TrafficCoordinationQueue"]          ?? "traffic.light.coordination";
        _exchangeName = section["Exchange:PriorityPublicTransport"]     ?? "priority.public.transport";
        _routingKeyPattern = section["RoutingKey:PriorityPublicKey"]    ?? "*";
    }

    public async Task Consume(ConsumeContext<PriorityPublicTransport> context)
    {
        var msg = context.Message;
        _logger.LogInformation("[PRIORITY] PublicTransport received on '{Exchange}' key '{Key}' -> intersection {Id}, active={Active}, ts={Ts}",
            _exchangeName, _routingKeyPattern, msg.IntersectionId, msg.PriorityActive, msg.Timestamp);

        var pattern = PatternBuilder.For("public_transport", msg.PriorityActive);
        await _publisher.PublishAsync(msg.IntersectionId, pattern, context.CancellationToken);
        _logger.LogInformation("[PRIORITY] PublicTransport applied -> {Pattern}", pattern);
    }
}
