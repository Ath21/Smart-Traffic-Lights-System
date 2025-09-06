using LogMessages;
using MassTransit;

namespace TrafficLightControllerStore.Publishers.Logs;

public class TrafficLogPublisher : ITrafficLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLogPublisher> _logger;
    private readonly string _serviceName = "traffic_light_controller_service";
    private readonly string _logExchange;
    private readonly string _auditKey;
    private readonly string _errorKey;

    private const string ServiceTag = "[" + nameof(TrafficLogPublisher) + "]";

    public TrafficLogPublisher(IConfiguration configuration, ILogger<TrafficLogPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _logExchange = configuration["RabbitMQ:Exchanges:Log"] ?? "LOG.EXCHANGE";
        _auditKey    = configuration["RabbitMQ:RoutingKeys:Log:Audit"] 
                       ?? "log.traffic.light_controller_service.audit";
        _errorKey    = configuration["RabbitMQ:RoutingKeys:Log:Error"] 
                       ?? "log.traffic.light_controller_service.error";
    }

    // log.traffic.light_controller_service.audit
    public async Task PublishAuditAsync(string action, string details, object? metadata = null)
    {
        var log = new AuditLogMessage(
            Guid.NewGuid(),
            _serviceName,
            action,
            details,
            DateTime.UtcNow,
            metadata
        );

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(_auditKey));

        _logger.LogInformation("{Tag} AUDIT published: {Action} -> {Details}", ServiceTag, action, details);
    }

    // log.traffic.light_controller_service.error
    public async Task PublishErrorAsync(string errorType, string message, object? metadata = null)
    {
        var log = new ErrorLogMessage(
            Guid.NewGuid(),
            _serviceName,
            errorType,
            message,
            DateTime.UtcNow,
            metadata
        );

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(_errorKey));

        _logger.LogError("{Tag} ERROR published: {Type} -> {Message}", ServiceTag, errorType, message);
    }
}
