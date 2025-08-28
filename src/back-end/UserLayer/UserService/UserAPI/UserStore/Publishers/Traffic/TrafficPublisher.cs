using MassTransit;
using TrafficMessages;

namespace UserStore.Publishers.Traffic;

public class TrafficPublisher : ITrafficPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficPublisher> _logger;
    private readonly string _controlKey;
    private readonly string _updateKey;

    private const string ServiceTag = "[" + nameof(TrafficPublisher) + "]";

    public TrafficPublisher(IConfiguration configuration, IBus bus, ILogger<TrafficPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        var section = configuration.GetSection("RabbitMQ:RoutingKeys");
        _controlKey = section["LightControl"] ?? "traffic.light.control.{intersection_id}.{light_id}";
        _updateKey  = section["LightUpdate"]  ?? "traffic.light.update.{intersection_id}";
    }

    // traffic.light.control.{intersection_id}.{light_id}
    public async Task PublishControlAsync(Guid intersectionId, Guid lightId, string newState)
    {
        var key = _controlKey
            .Replace("{intersection_id}", intersectionId.ToString())
            .Replace("{light_id}", lightId.ToString());

        var message = new TrafficLightControlMessage(
            intersectionId,
            lightId,
            newState,
            DateTime.UtcNow
        );

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(key));

        _logger.LogInformation(
            "{Tag} Control published: {IntersectionId}-{LightId} -> {State}",
            ServiceTag, intersectionId, lightId, newState);
    }

    // traffic.light.update.{intersection_id}
    public async Task PublishUpdateAsync(Guid intersectionId, Guid lightId, string currentState)
    {
        var key = _updateKey.Replace("{intersection_id}", intersectionId.ToString());

        var message = new TrafficLightUpdateMessage(
            intersectionId,
            lightId,
            currentState,
            DateTime.UtcNow
        );

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(key));

        _logger.LogInformation(
            "{Tag} Update published: {IntersectionId}-{LightId} state {State}",
            ServiceTag, intersectionId, lightId, currentState);
    }
}
