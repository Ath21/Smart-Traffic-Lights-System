using LogMessages;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DetectionStore.Publishers.Logs;

public class DetectionLogPublisher : IDetectionLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<DetectionLogPublisher> _logger;
    private readonly string _serviceName = "detection_service";
    private readonly string _auditKey;
    private readonly string _errorKey;

    private const string ServiceTag = "[" + nameof(DetectionLogPublisher) + "]";

    public DetectionLogPublisher(IConfiguration config, ILogger<DetectionLogPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _auditKey = config["RabbitMQ:RoutingKeys:Log:Audit"] ?? "log.sensor.detection_service.audit";
        _errorKey = config["RabbitMQ:RoutingKeys:Log:Error"] ?? "log.sensor.detection_service.error";
    }

    public async Task PublishAuditAsync(string action, string details, object? metadata = null)
    {
        var log = new LogMessage
        {
            Layer = "sensor",
            Service = _serviceName,
            Type = "audit",
            Message = $"{action}: {details}",
            Timestamp = DateTime.UtcNow,
            Metadata = metadata as Dictionary<string, object>
        };

        await _bus.Publish(log, ctx => ctx.SetRoutingKey(_auditKey));
        _logger.LogInformation("{Tag} AUDIT {Action} -> {Details}", ServiceTag, action, details);
    }

    public async Task PublishErrorAsync(string errorType, string message, object? metadata = null)
    {
        var log = new LogMessage
        {
            Layer = "sensor",
            Service = _serviceName,
            Type = "error",
            Message = $"{errorType}: {message}",
            Timestamp = DateTime.UtcNow,
            Metadata = metadata as Dictionary<string, object>
        };

        await _bus.Publish(log, ctx => ctx.SetRoutingKey(_errorKey));
        _logger.LogError("{Tag} ERROR {Type} -> {Message}", ServiceTag, errorType, message);
    }
}
