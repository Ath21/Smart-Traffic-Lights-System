using LogMessages;
using MassTransit;

namespace TrafficLightCoordinatorStore.Publishers.Logs;

public class TrafficLogPublisher : ITrafficLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLogPublisher> _logger;
    private readonly string _auditKey;
    private readonly string _errorKey;

    private const string ServiceTag = "[" + nameof(TrafficLogPublisher) + "]";

    public TrafficLogPublisher(IConfiguration config, ILogger<TrafficLogPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _auditKey = config["RabbitMQ:RoutingKeys:Audit"] 
                    ?? "log.traffic.light_coordinator_service.audit";
        _errorKey = config["RabbitMQ:RoutingKeys:Error"] 
                    ?? "log.traffic.light_coordinator_service.error";
    }

    public async Task PublishAuditAsync(string action, string details, object? metadata, CancellationToken ct)
    {
        var msg = new AuditLogMessage(
            Guid.NewGuid(),
            ServiceName: ServiceTag,
            Action: action,
            Details: details,
            Timestamp: DateTime.UtcNow,
            Metadata: metadata
        );

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(_auditKey), ct);

        _logger.LogInformation("{Tag} Published AUDIT {Action} -> {RoutingKey}", ServiceTag, action, _auditKey);
    }

    public async Task PublishErrorAsync(string errorType, string message, object? metadata, CancellationToken ct)
    {
        var msg = new ErrorLogMessage(
            Guid.NewGuid(),
            ServiceName: ServiceTag,
            ErrorType: errorType,
            Message: message,
            Timestamp: DateTime.UtcNow,
            Metadata: metadata
        );

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(_errorKey), ct);

        _logger.LogWarning("{Tag} Published ERROR {Type} -> {RoutingKey} | {Msg}", ServiceTag, errorType, _errorKey, message);
    }
}
