using MassTransit;
using Messages.Log;
using IntersectionControllerStore.Domain;

namespace IntersectionControllerStore.Publishers.Logs;

public class IntersectionLogPublisher : IIntersectionLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<IntersectionLogPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public IntersectionLogPublisher(
        IConfiguration config,
        ILogger<IntersectionLogPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:IntersectionController"]
                          ?? "log.traffic.intersection-controller.{type}";
    }

    public async Task PublishAuditAsync(string action, string message,
        List<int>? lightIds = null, List<string>? lights = null,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        var msg = CreateBaseLog("Audit", action, message, lightIds, lights, metadata, correlationId);
        await PublishAsync("audit", msg);
        _logger.LogInformation("[{Intersection}] AUDIT log published (Action={Action})", _intersection.Name, action);
    }

    public async Task PublishErrorAsync(string action, string errorMessage,
        Exception? ex = null, List<int>? lightIds = null, List<string>? lights = null,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        metadata ??= new();
        if (ex != null)
        {
            metadata["exception_type"] = ex.GetType().Name;
            metadata["exception_message"] = ex.Message;
        }

        var msg = CreateBaseLog("Error", action, errorMessage, lightIds, lights, metadata, correlationId);
        await PublishAsync("error", msg);
        _logger.LogError(ex, "[{Intersection}] ERROR log published (Action={Action}) - {Error}", _intersection.Name, action, errorMessage);
    }

    public async Task PublishFailoverAsync(string action, string message,
        List<int>? lightIds = null, List<string>? lights = null,
        Dictionary<string, string>? metadata = null, Guid? correlationId = null)
    {
        var msg = CreateBaseLog("Failover", action, message, lightIds, lights, metadata, correlationId);
        await PublishAsync("failover", msg);
        _logger.LogWarning("[{Intersection}] FAILOVER log published (Action={Action})", _intersection.Name, action);
    }

    private LogMessage CreateBaseLog(
        string logType,
        string action,
        string message,
        List<int>? lightIds,
        List<string>? lights,
        Dictionary<string, string>? metadata,
        Guid? correlationId)
    {
        return new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Layer = "Traffic",
            LogType = logType,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "Intersection Controller Service" },
            DestinationServices = new() { "Log Service" },
            LightId = lightIds,
            TrafficLight = lights,
            Action = action,
            Message = message,
            Metadata = metadata
        };
    }

    private async Task PublishAsync(string type, LogMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern.Replace("{type}", type);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
