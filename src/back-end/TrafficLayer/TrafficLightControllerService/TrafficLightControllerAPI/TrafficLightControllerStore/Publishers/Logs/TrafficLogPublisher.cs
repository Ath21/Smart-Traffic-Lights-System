using MassTransit;
using LogMessages;

namespace TrafficLightControllerStore.Publishers.Logs;

public class TrafficLogPublisher : ITrafficLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLogPublisher> _logger;
    private readonly string _serviceName = "traffic_light_controller_service";
    private readonly string _auditKey;
    private readonly string _errorKey;

    private const string ServiceTag = "[" + nameof(TrafficLogPublisher) + "]";

    public TrafficLogPublisher(IConfiguration configuration, ILogger<TrafficLogPublisher> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;

        var section = configuration.GetSection("RabbitMQ:RoutingKeys");
        _auditKey = section["TrafficLogsAudit"] ?? "log.traffic.light_controller_service.audit";
        _errorKey = section["TrafficLogsError"] ?? "log.traffic.light_controller_service.error";
    }

    // log.traffic.light_controller_service.audit
    public async Task PublishAuditAsync(string action, string details, object? metadata = null)
    {
        var message = new AuditLogMessage(
            Guid.NewGuid(),
            _serviceName,
            action,
            details,
            DateTime.UtcNow,
            metadata
        );

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(_auditKey));

        _logger.LogInformation("{Tag} Audit: {Action} -> {Details}", ServiceTag, action, details);
    }

    // log.traffic.light_controller_service.error
    public async Task PublishErrorAsync(string errorType, string message, object? metadata = null)
    {
        var messageText = new ErrorLogMessage(
            Guid.NewGuid(),
            _serviceName,
            errorType,
            message,
            DateTime.UtcNow,
            metadata
        );

        await _bus.Publish(message, ctx => ctx.SetRoutingKey(_errorKey));

        _logger.LogError("{Tag} Error: {ErrorType} - {Message}", ServiceTag, errorType, messageText);
    }
}
