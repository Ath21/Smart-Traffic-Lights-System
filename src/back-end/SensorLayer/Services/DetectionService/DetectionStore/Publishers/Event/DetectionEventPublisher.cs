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
    private readonly string _detectionKey;

    private const string ServiceTag = "[" + nameof(DetectionEventPublisher) + "]";

    public DetectionEventPublisher(IConfiguration config, ILogger<DetectionEventPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _detectionKey = config["RabbitMQ:RoutingKeys:Sensor:Detection"] 
            ?? "sensor.detection.{intersection}.{event}";
    }

    // Generic publisher for any detection event
    public async Task PublishDetectionAsync(int intersectionId, string eventType, string? details = null)
    {
        var routingKey = _detectionKey
            .Replace("{intersection}", intersectionId.ToString())
            .Replace("{event}", eventType);

        var msg = new SensorDetectionMessage
        {
            Intersection = intersectionId.ToString(),
            Event = eventType,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} Detection published for {IntersectionId}: {Event} ({Details})",
            ServiceTag, intersectionId, eventType, details ?? "N/A");
    }
}
