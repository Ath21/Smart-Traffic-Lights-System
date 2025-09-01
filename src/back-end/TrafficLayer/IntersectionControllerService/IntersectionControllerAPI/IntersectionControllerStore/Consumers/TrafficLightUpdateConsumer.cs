using System;
using MassTransit;
using TrafficMessages;

namespace IntersectionControllerStore.Consumers;

public class TrafficLightUpdateConsumer : IConsumer<TrafficLightUpdateMessage>
{
    private readonly ILogger<TrafficLightUpdateConsumer> _logger;
    private const string ServiceTag = "[" + nameof(TrafficLightUpdateConsumer) + "]";

    public TrafficLightUpdateConsumer(ILogger<TrafficLightUpdateConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<TrafficLightUpdateMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "{Tag} Received TrafficLightUpdate for Intersection {IntersectionId}, Light {LightId}: State={State} at {UpdatedAt}",
            ServiceTag, msg.IntersectionId, msg.LightId, msg.CurrentState, msg.UpdatedAt);

        // TODO: Update Redis state for this intersection/light

        return Task.CompletedTask;
    }
}