using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages;

namespace IntersectionControllerStore.Publishers.LightPub;

public class TrafficLightControlPublisher : ITrafficLightControlPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightControlPublisher> _logger;
    private readonly string _trafficExchange;
    private readonly string _lightControlKey;

    private const string ServiceTag = "[" + nameof(TrafficLightControlPublisher) + "]";

    public TrafficLightControlPublisher(IBus bus, ILogger<TrafficLightControlPublisher> logger, IConfiguration configuration)
    {
        _bus = bus;
        _logger = logger;

        _trafficExchange = configuration["RabbitMQ:Exchanges:Traffic"] ?? "TRAFFIC.EXCHANGE";
        _lightControlKey = configuration["RabbitMQ:RoutingKeys:Traffic:LightControl"] 
                           ?? "traffic.light.control.{intersection_id}.{light_id}";
    }

    public async Task PublishControlAsync(string intersection, string light, string newState)
    {
        var routingKey = _lightControlKey
            .Replace("{intersection}", intersection)
            .Replace("{light}", light);

        var message = new TrafficLightControlMessage(intersection, light, newState, DateTime.UtcNow);

        _logger.LogInformation(
            "{Tag} Publishing control for {Intersection}-{Light} -> {State} ({RoutingKey})",
            ServiceTag, intersection, light, newState, routingKey);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_trafficExchange}"));
        await endpoint.Send(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Control command published successfully", ServiceTag);
    }

}
