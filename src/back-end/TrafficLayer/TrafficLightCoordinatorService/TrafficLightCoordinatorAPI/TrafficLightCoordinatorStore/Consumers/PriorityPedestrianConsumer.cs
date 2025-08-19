using System;
using MassTransit;
using TrafficLightCoordinatorStore.Publishers.Update;
using TrafficMessages.Priority;

namespace TrafficLightCoordinatorStore.Consumers;

public class PriorityPedestrianConsumer : IConsumer<PriorityPedestrian>
{
    private readonly ILightUpdatePublisher _publisher;
    private readonly ILogger<PriorityPedestrianConsumer> _logger;

    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public PriorityPedestrianConsumer(
        ILightUpdatePublisher publisher,
        ILogger<PriorityPedestrianConsumer> logger,
        IConfiguration configuration)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var section = configuration.GetSection("RabbitMQ");
        _queueName = section["Queue:TrafficCoordinationQueue"]        ?? "traffic.light.coordination";
        _exchangeName = section["Exchange:PriorityPedestrian"]        ?? "priority.pedestrian";
        _routingKeyPattern = section["RoutingKey:PriorityPedestrianKey"] ?? "*";
    }

    public async Task Consume(ConsumeContext<PriorityPedestrian> context)
    {
        var msg = context.Message;
        _logger.LogInformation("[PRIORITY] Pedestrian received on '{Exchange}' key '{Key}' -> intersection {Id}, active={Active}, ts={Ts}",
            _exchangeName, _routingKeyPattern, msg.IntersectionId, msg.PriorityActive, msg.Timestamp);

        var pattern = PatternBuilder.For("pedestrian", msg.PriorityActive);
        await _publisher.PublishAsync(msg.IntersectionId, pattern, context.CancellationToken);
        _logger.LogInformation("[PRIORITY] Pedestrian applied -> {Pattern}", pattern);
    }
}
