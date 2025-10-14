using System;
using MassTransit;
using Messages.Traffic;

namespace IntersectionControllerStore.Publishers.Priority;

public class PriorityPublisher : IPriorityPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<PriorityPublisher> _logger;
    private readonly string _routingPatternCount;
    private readonly string _routingPatternEvent;

    public PriorityPublisher(IBus bus, IConfiguration config, ILogger<PriorityPublisher> logger)
    {
        _bus = bus;
        _logger = logger;
        _routingPatternCount = config["RabbitMQ:RoutingKeys:Traffic:PriorityCount"]
                               ?? "priority.count.{intersection}.{type}";
        _routingPatternEvent = config["RabbitMQ:RoutingKeys:Traffic:PriorityEvent"]
                               ?? "priority.detection.{intersection}.{event}";
    }

    public async Task PublishPriorityCountAsync(PriorityCountMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPatternCount.Replace("{intersection}", msg.IntersectionName!.ToLower().Replace(' ', '-'));
        routingKey = routingKey.Replace("{type}", msg.CountType.ToString().ToLower());
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[PUBLISHER][PRIORITY][COUNT][{Intersection}] {Type}={Count}, Priority={Level}",
            msg.IntersectionName, msg.CountType, msg.TotalCount, msg.PriorityLevel);
    }

    public async Task PublishPriorityEventAsync(PriorityEventMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPatternEvent.Replace("{intersection}", msg.IntersectionName!.ToLower().Replace(' ', '-'));
        routingKey = routingKey.Replace("{event}", msg.EventType.ToString().ToLower());
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("[PUBLISHER][PRIORITY][EVENT][{Intersection}] {EventType} ({VehicleType}) Priority={Level}",
            msg.IntersectionName, msg.EventType, msg.VehicleType, msg.PriorityLevel);
    }
}