// Publishers/LightUpdatePublisher.cs
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TrafficMessages.Light; // your records

namespace TrafficLightCoordinatorStore.Publishers.Update;

public class LightUpdatePublisher : ILightUpdatePublisher
{
    private readonly IBus _bus;
    private readonly ILogger<LightUpdatePublisher> _logger;
    private readonly string _updateExchange;

    public LightUpdatePublisher(IBus bus, IConfiguration config, ILogger<LightUpdatePublisher> logger)
    {
        _bus = bus;
        _logger = logger;
        _updateExchange = config.GetSection("RabbitMQ")["TrafficLightUpdateExchange"] ?? "traffic.light.update";
    }

    public async Task PublishAsync(string intersectionId, string currentPattern, CancellationToken ct)
    {
        _logger.LogInformation("[UPDATE] Publishing TrafficLightStateUpdate to '{Exchange}' with routing key '{Key}'",
            _updateExchange, intersectionId);

        await _bus.Publish(new TrafficLightStateUpdate(
            IntersectionId: intersectionId,
            CurrentPattern: currentPattern,
            Timestamp: DateTime.UtcNow),
            context =>
            {
                // topic route: traffic.light.update.<intersection_id>
                context.SetRoutingKey(intersectionId);
            }, ct);

        _logger.LogInformation("[UPDATE] Published schedule for intersection {Id}: {Pattern}", intersectionId, currentPattern);
    }
}
