using MassTransit;
using Microsoft.Extensions.Logging;
using TrafficMessages;

namespace UserStore.Publishers.Traffic;

public class TrafficPublisher : ITrafficPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficPublisher> _logger;

    public TrafficPublisher(IBus bus, ILogger<TrafficPublisher> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task PublishControlAsync(Guid intersectionId, Guid lightId, string newState)
    {
        var key = $"traffic.light.control.{intersectionId}.{lightId}";

        var message = new TrafficLightControlMessage(
            intersectionId,
            lightId,
            newState,
            DateTime.UtcNow
        );

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(key));

        _logger.LogInformation("Traffic control command published: {IntersectionId}-{LightId} -> {State}",
            intersectionId, lightId, newState);
    }

    public async Task PublishUpdateAsync(Guid intersectionId, Guid lightId, string currentState)
    {
        var key = $"traffic.light.update.{intersectionId}";

        var message = new TrafficLightUpdateMessage(
            intersectionId,
            lightId,
            currentState,
            DateTime.UtcNow
        );

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(key));

        _logger.LogInformation("Traffic update published: {IntersectionId}-{LightId} state {State}",
            intersectionId, lightId, currentState);
    }
}
