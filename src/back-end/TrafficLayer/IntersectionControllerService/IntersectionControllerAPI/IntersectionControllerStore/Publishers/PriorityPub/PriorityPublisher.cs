using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrafficMessages;

namespace IntersectionControlStore.Publishers.PriorityPub;

public class PriorityPublisher : IPriorityPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<PriorityPublisher> _logger;
    private readonly string _trafficExchange;
    private readonly string _priorityRoutingKeyBase;

    private const string ServiceTag = "[" + nameof(PriorityPublisher) + "]";

    public PriorityPublisher(IBus bus, ILogger<PriorityPublisher> logger, IConfiguration configuration)
    {
        _bus = bus;
        _logger = logger;

        _trafficExchange = configuration["RabbitMQ:Exchanges:Traffic"] ?? "TRAFFIC.EXCHANGE";
        _priorityRoutingKeyBase = configuration["RabbitMQ:RoutingKeys:Priority"] ?? "traffic.priority.{priorityType}.{intersection_id}";
    }

    public async Task PublishPriorityAsync(Guid intersectionId, string priorityType, Guid? detectionId, string? reason)
    {
        var routingKey = _priorityRoutingKeyBase
            .Replace("{priorityType}", priorityType)
            .Replace("{intersection_id}", intersectionId.ToString());

        var message = new PriorityMessage(intersectionId, priorityType, detectionId, reason, DateTime.UtcNow);

        _logger.LogInformation("{Tag} Publishing {Type} priority for {IntersectionId} to {RoutingKey}",
            ServiceTag, priorityType, intersectionId, routingKey);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_trafficExchange}"));
        await endpoint.Send(message, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Priority {Type} published successfully for {IntersectionId}",
            ServiceTag, priorityType, intersectionId);
    }
}