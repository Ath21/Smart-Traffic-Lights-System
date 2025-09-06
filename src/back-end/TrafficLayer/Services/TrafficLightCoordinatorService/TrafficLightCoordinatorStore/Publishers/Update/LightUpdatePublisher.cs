using MassTransit;
using TrafficMessages;

namespace TrafficLightCoordinatorStore.Publishers.Update;

public class LightUpdatePublisher : ILightUpdatePublisher
{
    private readonly IBus _bus;
    private readonly ILogger<LightUpdatePublisher> _logger;
    private readonly string _updateKey;

    private const string ServiceTag = "[" + nameof(LightUpdatePublisher) + "]";

    public LightUpdatePublisher(IConfiguration config, ILogger<LightUpdatePublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _updateKey = config["RabbitMQ:RoutingKeys:Traffic:LightUpdate"] 
                     ?? "traffic.light.update.{intersection_id}";
    }

    // traffic.light.update.{intersection_id}
    public async Task PublishAsync(TrafficLightUpdateMessage message, CancellationToken ct)
    {
        var routingKey = _updateKey.Replace("{intersection_id}", message.IntersectionId.ToString());

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(routingKey), ct);

        _logger.LogInformation(
            "{Tag} Published Light Update for Intersection {IntersectionId} Light {LightId} -> {State} ({RoutingKey})",
            ServiceTag, message.IntersectionId, message.LightId, message.CurrentState, routingKey
        );
    }
}
