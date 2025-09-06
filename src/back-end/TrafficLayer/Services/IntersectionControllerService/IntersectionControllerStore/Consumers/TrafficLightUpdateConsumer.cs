using System;
using IntersectionControllerStore.Business.Coordinator;
using MassTransit;
using TrafficMessages;

namespace IntersectionControllerStore.Consumers;

public class TrafficLightUpdateConsumer : IConsumer<TrafficLightUpdateMessage>
{
    private readonly ILogger<TrafficLightUpdateConsumer> _logger;
    private readonly ITrafficLightCoordinatorService _coordinator;
    private const string ServiceTag = "[" + nameof(TrafficLightUpdateConsumer) + "]";

    public TrafficLightUpdateConsumer(
        ILogger<TrafficLightUpdateConsumer> logger,
        ITrafficLightCoordinatorService coordinator)
    {
        _logger = logger;
        _coordinator = coordinator;
    }

    public async Task Consume(ConsumeContext<TrafficLightUpdateMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Received update for Intersection {IntersectionId}, Light {LightId}: {State} at {UpdatedAt}",
            ServiceTag, msg.IntersectionId, msg.LightId, msg.CurrentState, msg.UpdatedAt);

        await _coordinator.ApplyUpdateAsync(msg.IntersectionId, msg.LightId, msg.CurrentState, msg.UpdatedAt);
    }
}