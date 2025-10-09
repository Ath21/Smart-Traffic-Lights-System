using System;
using IntersectionControllerStore.Domain;
using MassTransit;
using Messages.Traffic;

namespace IntersectionControllerStore.Publishers.Priority;

public class PriorityEventPublisher : IPriorityEventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<PriorityEventPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public PriorityEventPublisher(
        IConfiguration config,
        ILogger<PriorityEventPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Traffic:PriorityEvent"]
                          ?? "priority.detection.{intersection}.{event}";
    }

    public async Task PublishEventAsync(string eventType, string vehicleType, int priorityLevel,
        string direction, Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        var msg = new PriorityEventMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            EventType = eventType,
            VehicleType = vehicleType,
            PriorityLevel = priorityLevel,
            Direction = direction,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "Intersection Controller Service" },
            DestinationServices = new() { "Traffic Light Coordinator Service" },
            Metadata = metadata
        };

        await PublishAsync(eventType.ToLower(), msg);

        _logger.LogInformation("[{Intersection}] PRIORITY EVENT ({Event}) published: {Vehicle} ({Direction}, Level={Level})",
            _intersection.Name, eventType, vehicleType, direction, priorityLevel);
    }

    private async Task PublishAsync(string eventKey, PriorityEventMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern
            .Replace("{intersection}", _intersection.Name.ToLower().Replace(' ', '-'))
            .Replace("{event}", eventKey);

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
