using MassTransit;
using Messages.Log;
using TrafficLightControllerStore.Domain;

namespace TrafficLightControllerStore.Publishers.Logs;

public class TrafficLightControllerLogPublisher : ITrafficLightControllerLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightControllerLogPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly TrafficLightContext _trafficLightContext;
    private readonly string _routingPattern;

    public TrafficLightControllerLogPublisher(
        IConfiguration config,
        ILogger<TrafficLightControllerLogPublisher> logger,
        IBus bus,
        IntersectionContext intersection,
        TrafficLightContext trafficLightContext)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;
        _trafficLightContext = trafficLightContext;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:TrafficLightController"]
                          ?? "log.traffic.traffic-light-controller.{type}";
    }

    // ============================================================
    // Publish AUDIT log
    // ============================================================
    public async Task PublishAuditAsync(
        string action,
        string message,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        var msg = CreateLog("Audit", action, message, metadata, correlationId);
        await PublishAsync("audit", msg);

        _logger.LogInformation("[{Intersection}/{Light}] AUDIT log → {Action}", 
            _intersection.Name, _trafficLightContext.Name, action);
    }

    // ============================================================
    // Publish ERROR log
    // ============================================================
    public async Task PublishErrorAsync(
        string action,
        string errorMessage,
        Exception? ex = null,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        metadata ??= new();
        if (ex != null)
        {
            metadata["exception_type"] = ex.GetType().Name;
            metadata["exception_message"] = ex.Message;
        }

        var msg = CreateLog("Error", action, errorMessage, metadata, correlationId);
        await PublishAsync("error", msg);

        _logger.LogError(ex, "[{Intersection}/{Light}] ERROR log → {Action} | {Error}", 
            _intersection.Name, _trafficLightContext.Name, action, errorMessage);
    }

    // ============================================================
    // Publish FAILOVER log
    // ============================================================
    public async Task PublishFailoverAsync(
        string action,
        string message,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        var msg = CreateLog("Failover", action, message, metadata, correlationId);
        await PublishAsync("failover", msg);

        _logger.LogWarning("[{Intersection}/{Light}] FAILOVER log → {Action}", 
            _intersection.Name, _trafficLightContext.Name, action);
    }

    // ============================================================
    // Internal helper methods
    // ============================================================
    private LogMessage CreateLog(
        string logType,
        string action,
        string message,
        Dictionary<string, string>? metadata,
        Guid? correlationId)
    {
        return new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,
            Layer = "Traffic",
            LogType = logType,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            SourceServices = new() { "Traffic Light Controller Service" },
            DestinationServices = new() { "Log Service" },
            LightId = new() { _trafficLightContext.Id },
            TrafficLight = new() { _trafficLightContext.Name },
            Action = action,
            Message = message,
            Metadata = metadata
        };
    }

    private async Task PublishAsync(string logType, LogMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;

        var routingKey = _routingPattern.Replace("{type}", logType);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}