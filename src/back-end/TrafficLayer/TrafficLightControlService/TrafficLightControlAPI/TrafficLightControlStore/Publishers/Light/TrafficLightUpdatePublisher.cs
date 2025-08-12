using System;
using MassTransit;

namespace TrafficLightControlStore.Publishers.Light;

public class TrafficLightUpdatePublisher : ITrafficLightUpdatePublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightUpdatePublisher> _logger;
    private readonly string _exchange;

    public TrafficLightUpdatePublisher(IBus bus, ILogger<TrafficLightUpdatePublisher> logger, IConfiguration config)
   {
        _bus = bus;
        _logger = logger;
        _exchange = config["RabbitMQ:Exchange:TrafficLightUpdateExchange"] ?? "traffic.light.update.exchange";
    }

    public async Task PublishUpdateAsync(string intersectionId, string state)
    {
        var routingKey = $"traffic.light.update.{intersectionId}";
        var sendEndpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_exchange}"));

        await sendEndpoint.Send(new { IntersectionId = intersectionId, State = state, Timestamp = DateTime.UtcNow },
            context => context.SetRoutingKey(routingKey));

        _logger.LogInformation("[PUBLISH] Light update {State} for {IntersectionId}", state, intersectionId);
    }
}
