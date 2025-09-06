using MassTransit;
using TrafficMessages;

namespace UserStore.Publishers.Traffic;

public class TrafficPublisher : ITrafficPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficPublisher> _logger;
    private readonly string _controlKeyTemplate;
    private readonly string _updateKeyTemplate;

    private const string ServiceTag = "[" + nameof(TrafficPublisher) + "]";

    public TrafficPublisher(IConfiguration configuration, IBus bus, ILogger<TrafficPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        _controlKeyTemplate = configuration["RabbitMQ:RoutingKeys:Traffic:LightControl"]
                              ?? "traffic.light.control.{intersection_id}.{light_id}";

        _updateKeyTemplate = configuration["RabbitMQ:RoutingKeys:Traffic:LightUpdate"]
                             ?? "traffic.light.update.{intersection_id}";
    }

    public async Task PublishControlAsync(Guid intersectionId, Guid lightId, string newState)
    {
        var key = _controlKeyTemplate
            .Replace("{intersection_id}", intersectionId.ToString())
            .Replace("{light_id}", lightId.ToString());

        var message = new TrafficLightControlMessage(intersectionId, lightId, newState, DateTime.UtcNow);
        await _bus.Publish(message, ctx => ctx.SetRoutingKey(key));

        _logger.LogInformation("{Tag} Control published: {IntersectionId}-{LightId} -> {State}", ServiceTag, intersectionId, lightId, newState);
    }

    public async Task PublishUpdateAsync(Guid intersectionId, Guid lightId, string currentState)
    {
        var key = _updateKeyTemplate.Replace("{intersection_id}", intersectionId.ToString());

        var message = new TrafficLightUpdateMessage(intersectionId, lightId, currentState, DateTime.UtcNow);
        await _bus.Publish(message, ctx => ctx.SetRoutingKey(key));

        _logger.LogInformation("{Tag} Update published: {IntersectionId}-{LightId} state {State}", ServiceTag, intersectionId, lightId, currentState);
    }
}
