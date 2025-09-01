using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages;

namespace IntersectionControlStore.Publishers.LightPub;

public class TrafficLightControlPublisher : ITrafficLightControlPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightControlPublisher> _logger;
    private readonly string _trafficExchange;
    private readonly string _lightControlRoutingKeyBase;

    private const string ServiceTag = "[" + nameof(TrafficLightControlPublisher) + "]";

    public TrafficLightControlPublisher(IBus bus, ILogger<TrafficLightControlPublisher> logger, IConfiguration configuration)
    {
        _bus = bus;
        _logger = logger;

        _trafficExchange = configuration["RabbitMQ:Exchanges:Traffic"] ?? "TRAFFIC.EXCHANGE";
        _lightControlRoutingKeyBase = configuration["RabbitMQ:RoutingKeys:TrafficLightControl"] ?? "traffic.light.control.{intersection_id}.{light_id}";
    }

    public async Task PublishControlAsync(Guid intersectionId, Guid lightId, string newState)
    {
        var routingKey = _lightControlRoutingKeyBase
            .Replace("{intersection_id}", intersectionId.ToString())
            .Replace("{light_id}", lightId.ToString());

        var message = new TrafficLightControlMessage(intersectionId, lightId, newState, DateTime.UtcNow);

        _logger.LogInformation("{Tag} Publishing control for light {LightId} -> {State} at intersection {IntersectionId} to {RoutingKey}",
            ServiceTag, lightId, newState, intersectionId, routingKey);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_trafficExchange}"));
        await endpoint.Send(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Control command published successfully for light {LightId} at intersection {IntersectionId}",
            ServiceTag, lightId, intersectionId);
    }
}
