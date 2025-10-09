using System;
using IntersectionControllerStore.Domain;
using MassTransit;
using Messages.Traffic;

namespace IntersectionControllerStore.Publishers.Priority;

public class PriorityCountPublisher : IPriorityCountPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<PriorityCountPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public PriorityCountPublisher(
        IConfiguration config,
        ILogger<PriorityCountPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Traffic:PriorityCount"]
                          ?? "priority.count.{intersection}.{type}";
    }

    public async Task PublishCountAsync(string type, int totalCount, int priorityLevel, bool thresholdExceeded,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        var msg = new PriorityCountMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            CountType = type,
            TotalCount = totalCount,
            PriorityLevel = priorityLevel,
            IsThresholdExceeded = thresholdExceeded,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "Intersection Controller Service" },
            DestinationServices = new() { "Traffic Light Coordinator Service" },
            Metadata = metadata
        };

        await PublishAsync(type.ToLower(), msg);

        _logger.LogInformation("[{Intersection}] PRIORITY COUNT ({Type}) published: Count={Total}, Level={Level}, Threshold={Exceeded}",
            _intersection.Name, type, totalCount, priorityLevel, thresholdExceeded);
    }

    private async Task PublishAsync(string typeKey, PriorityCountMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern
            .Replace("{intersection}", _intersection.Name.ToLower().Replace(' ', '-'))
            .Replace("{type}", typeKey);

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }

}
