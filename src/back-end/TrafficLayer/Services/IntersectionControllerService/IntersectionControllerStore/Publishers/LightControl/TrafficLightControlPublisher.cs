using System;
using MassTransit;
using Messages.Traffic;

namespace IntersectionControllerStore.Publishers.LightControl;

public class TrafficLightControlPublisher : ITrafficLightControlPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightControlPublisher> _logger;
    private readonly string _routingPattern;

    public TrafficLightControlPublisher(IBus bus, IConfiguration config, ILogger<TrafficLightControlPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        _routingPattern = config["RabbitMQ:RoutingKeys:Traffic:LightControl"]
                          ?? "traffic.light.control.{intersection}.{light}";
    }

    public async Task PublishControlAsync(TrafficLightControlMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern
            .Replace("{intersection}", msg.IntersectionName!.ToLower().Replace(' ', '-'))
            .Replace("{light}", msg.LightId.ToString());

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "[PUBLISHER][CONTROL][{Intersection}:{Light}] Mode={Mode}, Phase={Phase}, Remaining={Remaining}s, Offset={Offset}s",
            msg.IntersectionName, msg.LightId, msg.Mode, msg.CurrentPhase, msg.RemainingTimeSec, msg.LocalOffsetSec);
    }
}
