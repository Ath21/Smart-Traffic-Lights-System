using System;
using MassTransit;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using TrafficLightCoordinatorStore.Publishers.Update;
using TrafficMessages.Priority;

namespace TrafficLightCoordinatorStore.Consumers;

public class PriorityEmergencyVehicleConsumer : IConsumer<PriorityEmergencyVehicle>
{
    private readonly ILightUpdatePublisher _publisher;
    private readonly ILogger<PriorityEmergencyVehicleConsumer> _logger;

    private readonly string _queueName;
    private readonly string _exchangeName;
    private readonly string _routingKeyPattern;

    public PriorityEmergencyVehicleConsumer(
        ILightUpdatePublisher publisher,
        ILogger<PriorityEmergencyVehicleConsumer> logger,
        IConfiguration configuration)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var section = configuration.GetSection("RabbitMQ");
        _queueName = section["Queue:TrafficCoordinationQueue"]           ?? "traffic.light.coordination";
        _exchangeName = section["Exchange:PriorityEmergencyVehicle"]     ?? "priority.emergency.vehicle";
        _routingKeyPattern = section["RoutingKey:PriorityEmergencyKey"]  ?? "*";
    }

    public async Task Consume(ConsumeContext<PriorityEmergencyVehicle> context)
    {
        var msg = context.Message;
        _logger.LogInformation("[PRIORITY] EmergencyVehicle received on '{Exchange}' key '{Key}' -> intersection {Id}, active={Active}, ts={Ts}",
            _exchangeName, _routingKeyPattern, msg.IntersectionId, msg.PriorityActive, msg.Timestamp);

        var pattern = PatternBuilder.For("emergency", msg.PriorityActive);
        await _publisher.PublishAsync(msg.IntersectionId, pattern, context.CancellationToken);
        _logger.LogInformation("[PRIORITY] EmergencyVehicle applied -> {Pattern}", pattern);
    }
}
