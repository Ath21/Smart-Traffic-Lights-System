using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficMessages;
using TrafficLightControlStore.Repository;

namespace TrafficLightControlStore.Consumers;

/// <summary>
/// Consumes control commands for specific traffic lights and applies them to Redis.
/// </summary>
public class TrafficLightControlConsumer : IConsumer<TrafficLightControlMessage>
{
    private readonly ILogger<TrafficLightControlConsumer> _logger;
    private readonly ITrafficLightRepository _repository;

    public TrafficLightControlConsumer(
        ILogger<TrafficLightControlConsumer> logger,
        ITrafficLightRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<TrafficLightControlMessage> context)
    {
        var msg = context.Message;

        _logger.LogInformation(
            "[TrafficLightControlConsumer] Received CONTROL → Intersection={IntersectionId}, Light={LightId}, NewState={State}, IssuedAt={IssuedAt}",
            msg.IntersectionId, msg.LightId, msg.NewState, msg.IssuedAt);

        // 1. Update Redis with the new light state
        await _repository.SetLightStateAsync(msg.IntersectionId, msg.LightId, msg.NewState);

        // 2. Save control command for audit/debug
        await _repository.SaveControlEventAsync(msg.IntersectionId, msg.LightId, $"Control:{msg.NewState}");

        _logger.LogInformation(
            "[TrafficLightControlConsumer] Applied CONTROL → {IntersectionId}-{LightId} set to {State}",
            msg.IntersectionId, msg.LightId, msg.NewState);
    }
}
