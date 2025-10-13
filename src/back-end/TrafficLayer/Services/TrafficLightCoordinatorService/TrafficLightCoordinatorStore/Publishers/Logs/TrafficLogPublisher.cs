using LogMessages;
using MassTransit;

namespace TrafficLightCoordinatorStore.Publishers.Logs;

public class TrafficLogPublisher : ITrafficLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLogPublisher> _logger;
    private readonly string _auditKey;
    private readonly string _errorKey;
    private readonly string _serviceName = "traffic_light_coordinator_service";

    private const string ServiceTag = "[" + nameof(TrafficLogPublisher) + "]";

    public TrafficLogPublisher(IConfiguration config, ILogger<TrafficLogPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _auditKey = config["RabbitMQ:RoutingKeys:Log:Audit"] 
                    ?? "log.traffic.coordinator_service.audit";
        _errorKey = config["RabbitMQ:RoutingKeys:Log:Error"] 
                    ?? "log.traffic.coordinator_service.error";
    }

    public async Task PublishAuditAsync(string action, string details, object? metadata, CancellationToken ct)
    {
        var msg = new AuditLogMessage(
            Guid.NewGuid(),
            _serviceName,
            action,
            details,
            DateTime.UtcNow,
            metadata
        );

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(_auditKey), ct);

        _logger.LogInformation(
            "{Tag} Published AUDIT {Action} -> {RoutingKey}",
            ServiceTag, action, _auditKey
        );
    }

    public async Task PublishErrorAsync(string errorType, string message, object? metadata, CancellationToken ct)
    {
        var msg = new ErrorLogMessage(
            Guid.NewGuid(),
            _serviceName,
            errorType,
            message,
            DateTime.UtcNow,
            metadata
        );

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(_errorKey), ct);

        _logger.LogError(
            "{Tag} Published ERROR {Type} -> {RoutingKey} | {Msg}",
            ServiceTag, errorType, _errorKey, message
        );
    }
}
