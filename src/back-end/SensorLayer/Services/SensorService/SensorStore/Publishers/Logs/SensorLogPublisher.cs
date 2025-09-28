using LogMessages;
using MassTransit;

namespace SensorStore.Publishers.Logs;

public class SensorLogPublisher : ISensorLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<SensorLogPublisher> _logger;
    private readonly string _auditKey;
    private readonly string _errorKey;
    private const string Layer = "sensor";
    private const string ServiceName = "sensor_service";
    private const string ServiceTag = "[" + nameof(SensorLogPublisher) + "]";

    public SensorLogPublisher(IConfiguration config, ILogger<SensorLogPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _auditKey = config["RabbitMQ:RoutingKeys:Log:Audit"] ?? "log.sensor.sensor_service.{intersection}.audit";
        _errorKey = config["RabbitMQ:RoutingKeys:Log:Error"] ?? "log.sensor.sensor_service.{intersection}.error";
    }

    public async Task PublishAuditAsync(int intersectionId, string action, string details, object? metadata = null)
    {
        var routingKey = _auditKey.Replace("{intersection}", intersectionId.ToString());

        var log = new LogMessage
        {
            Layer = Layer,
            Service = ServiceName,
            Type = "audit",
            Message = $"{action}: {details}",
            Timestamp = DateTime.UtcNow,
            Metadata = metadata as Dictionary<string, object>
        };

        await _bus.Publish(log, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogInformation("{Tag} AUDIT {Action} @ {Intersection} -> {Details}", 
            ServiceTag, action, intersectionId, details);
    }

    public async Task PublishErrorAsync(int intersectionId, string errorType, string message, object? metadata = null)
    {
        var routingKey = _errorKey.Replace("{intersection}", intersectionId.ToString());

        var log = new LogMessage
        {
            Layer = Layer,
            Service = ServiceName,
            Type = "error",
            Message = $"{errorType}: {message}",
            Timestamp = DateTime.UtcNow,
            Metadata = metadata as Dictionary<string, object>
        };

        await _bus.Publish(log, ctx => ctx.SetRoutingKey(routingKey));

        _logger.LogError("{Tag} ERROR {Type} @ {Intersection} -> {Message}", 
            ServiceTag, errorType, intersectionId, message);
    }
}
