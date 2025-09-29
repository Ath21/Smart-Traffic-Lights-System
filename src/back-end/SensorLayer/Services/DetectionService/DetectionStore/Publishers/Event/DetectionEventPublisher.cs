using DetectionStore.Domain;
using DetectionStore.Publishers.Event;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorMessages;

namespace DetectionStore.Publishers.Events;

public class DetectionEventPublisher : IDetectionEventPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<DetectionEventPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _detectionKey;

    private const string ServiceTag = "[" + nameof(DetectionEventPublisher) + "]";

    public DetectionEventPublisher(
        IConfiguration config,
        ILogger<DetectionEventPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _detectionKey = config["RabbitMQ:RoutingKeys:Sensor:Detection"] 
                        ?? "sensor.detection.{intersection}.{event}";
    }

    // Publishes detection event scoped to THIS intersection
    public async Task PublishDetectionAsync(string eventType, string? details = null)
    {
        var routingKey = _detectionKey
            .Replace("{intersection}", _intersection.Id.ToString())
            .Replace("{event}", eventType);

        var msg = new SensorDetectionMessage
        {
            Intersection = _intersection.Id.ToString(),
            Event = eventType,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation(
            "{Tag} Detection published @ {IntersectionName} (Id={IntersectionId}): {Event} ({Details})",
            ServiceTag, _intersection.Name, _intersection.Id, eventType, details ?? "N/A"
        );
    }
}
