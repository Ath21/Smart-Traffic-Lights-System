using System;
using MassTransit;
using Messages.Log;

namespace TrafficLightControllerStore.Publishers.Logs;

public class TrafficLightLogPublisher : ITrafficLightLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLightLogPublisher> _logger;
    private readonly string _routingPattern;
    private readonly string _hostname;
    private readonly string _environment;

    public TrafficLightLogPublisher(
        IBus bus,
        IConfiguration config,
        ILogger<TrafficLightLogPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:LightController"]
                          ?? "log.traffic.light-controller.{type}";

        _hostname = Environment.MachineName;
        _environment = config["ASPNETCORE_ENVIRONMENT"] ?? "unknown";
    }

    // ============================================================
    // AUDIT
    // ============================================================
    public async Task PublishAuditAsync(
        string operation,
        string message,
        Dictionary<string, object>? data = null,
        string? correlationId = null)
    {
        var log = CreateBaseLog("audit", operation, message, data, correlationId);
        await PublishAsync("audit", log);
        _logger.LogInformation("[LOG][AUDIT] {Op} - {Msg}", operation, message);
    }

    // ============================================================
    // ERROR
    // ============================================================
    public async Task PublishErrorAsync(
        string operation,
        string message,
        Exception? ex = null,
        Dictionary<string, object>? data = null,
        string? correlationId = null)
    {
        data ??= new();
        if (ex != null)
        {
            data["exception_type"] = ex.GetType().Name;
            data["exception_message"] = ex.Message;
        }

        var log = CreateBaseLog("error", operation, message, data, correlationId);
        await PublishAsync("error", log);
        _logger.LogError(ex, "[LOG][ERROR] {Op} - {Msg}", operation, message);
    }

    // ============================================================
    // FAILOVER
    // ============================================================
    public async Task PublishFailoverAsync(
        string operation,
        string message,
        Dictionary<string, object>? data = null,
        string? correlationId = null)
    {
        var log = CreateBaseLog("failover", operation, message, data, correlationId);
        await PublishAsync("failover", log);
        _logger.LogWarning("[LOG][FAILOVER] {Op} - {Msg}", operation, message);
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================
    private LogMessage CreateBaseLog(string type, string operation, string message, Dictionary<string, object>? data, string? correlationId)
        => new()
        {
            Layer = "Traffic",
            Level = "Edge",
            Service = "TrafficLightController",
            Domain = "[CONTROLLER][TRAFFIC_LIGHT]",
            Type = type,
            Category = "system",
            Message = message,
            Operation = operation,
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Hostname = _hostname,
            Environment = _environment,
            Data = data
        };

    private async Task PublishAsync(string type, LogMessage msg)
    {
        var routingKey = _routingPattern.Replace("{type}", type);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
