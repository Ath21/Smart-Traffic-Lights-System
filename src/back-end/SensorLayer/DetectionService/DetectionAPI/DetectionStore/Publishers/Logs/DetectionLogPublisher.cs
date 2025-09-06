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
    private readonly string _logExchange;
    private readonly string _auditKey;
    private readonly string _errorKey;

    private const string ServiceTag = "[" + nameof(DetectionLogPublisher) + "]";

    public DetectionLogPublisher(IConfiguration config, ILogger<DetectionLogPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _logExchange = config["RabbitMQ:Exchanges:Log"] ?? "LOG.EXCHANGE";
        _auditKey    = config["RabbitMQ:RoutingKeys:Log:Audit"] ?? "log.sensor.detection_service.audit";
        _errorKey    = config["RabbitMQ:RoutingKeys:Log:Error"] ?? "log.sensor.detection_service.error";
    }

    // log.sensor.detection_service.audit
    public async Task PublishAuditAsync(string action, string details, object? metadata = null)
    {
        var log = new AuditLogMessage(Guid.NewGuid(), _serviceName, action, details, DateTime.UtcNow, metadata);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(_auditKey));

        _logger.LogInformation("{Tag} AUDIT {Action} -> {Details}", ServiceTag, action, details);
    }

    // log.sensor.detection_service.error
    public async Task PublishErrorAsync(string errorType, string message, object? metadata = null)
    {
        var log = new ErrorLogMessage(Guid.NewGuid(), _serviceName, errorType, message, DateTime.UtcNow, metadata);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(_errorKey));

        _logger.LogError("{Tag} ERROR {Type} -> {Message}", ServiceTag, errorType, message);
    }
}
