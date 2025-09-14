using LogMessages;
using MassTransit;

namespace TrafficLightControllerStore.Publishers.Logs;

public class TrafficLogPublisher : ITrafficLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLogPublisher> _logger;
    private readonly string _serviceName = "Traffic Light Controller Service";
    private readonly string _logExchange;
    private readonly string _auditKeyTemplate;
    private readonly string _errorKeyTemplate;

    private const string ServiceTag = "[" + nameof(TrafficLogPublisher) + "]";

    public TrafficLogPublisher(IConfiguration configuration, ILogger<TrafficLogPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _logExchange     = configuration["RabbitMQ:Exchanges:Log"] ?? "LOG.EXCHANGE";
        _auditKeyTemplate = configuration["RabbitMQ:RoutingKeys:Log:Audit"] 
                            ?? "log.traffic.light_controller.{intersection}.{light}.audit";
        _errorKeyTemplate = configuration["RabbitMQ:RoutingKeys:Log:Error"] 
                            ?? "log.traffic.light_controller.{intersection}.{light}.error";
    }

    private async Task PublishAsync<T>(T message, string routingKey)
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(message, ctx => ctx.SetRoutingKey(routingKey));
    }

    // Publishes an audit log with dynamic intersection/light routing
    public async Task PublishAuditAsync(
        string action,
        string details,
        string intersection,
        string light,
        object? metadata = null)
    {
        var log = new AuditLogMessage(
            Guid.NewGuid(),
            _serviceName,
            action,
            details,
            DateTime.UtcNow,
            metadata
        );

        var routingKey = _auditKeyTemplate
            .Replace("{intersection}", intersection)
            .Replace("{light}", light);

        await PublishAsync(log, routingKey);

        _logger.LogInformation(
            "{Tag} AUDIT published: Intersection={Intersection}, Light={Light}, Action={Action}, Details={Details}, Metadata={Metadata}",
            ServiceTag, intersection, light, action, details, metadata ?? "{}"
        );
    }

    // Publishes an error log with dynamic intersection/light routing
    public async Task PublishErrorAsync(
        string errorType,
        string message,
        string intersection,
        string light,
        object? metadata = null)
    {
        var log = new ErrorLogMessage(
            Guid.NewGuid(),
            _serviceName,
            errorType,
            message,
            DateTime.UtcNow,
            metadata
        );

        var routingKey = _errorKeyTemplate
            .Replace("{intersection}", intersection)
            .Replace("{light}", light);

        await PublishAsync(log, routingKey);

        _logger.LogError(
            "{Tag} ERROR published: Intersection={Intersection}, Light={Light}, Type={Type}, Message={Message}, Metadata={Metadata}",
            ServiceTag, intersection, light, errorType, message, metadata ?? "{}"
        );
    }
}
