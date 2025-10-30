using System;
using MassTransit;
using Messages.Log;

namespace IntersectionControllerStore.Publishers.Logs;

public class IntersectionLogPublisher : IIntersectionLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<IntersectionLogPublisher> _logger;
    private readonly string _routingPattern;
    private readonly string _hostname;
    private readonly string _environment;

    public IntersectionLogPublisher(
        IBus bus,
        IConfiguration config,
        ILogger<IntersectionLogPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:Intersection"]
                          ?? "log.traffic.intersection-controller.*";

        _hostname = Environment.MachineName;
        _environment = config["ASPNETCORE_ENVIRONMENT"] ?? "unknown";
    }

    public async Task PublishAuditAsync(string operation, string message, Dictionary<string, object>? data = null, string? correlationId = null)
    {
        var log = CreateBaseLog("audit", "system", operation, message, data, correlationId);
        await PublishAsync("audit", log);
        _logger.LogInformation("[LOG][AUDIT] {Op} - {Msg}", operation, message);
    }

    public async Task PublishErrorAsync(string operation, string message, Exception? ex = null, Dictionary<string, object>? data = null, string? correlationId = null)
    {
        data ??= new();
        if (ex != null)
        {
            data["exception_type"] = ex.GetType().Name;
            data["exception_message"] = ex.Message;
        }

        var log = CreateBaseLog("error", "system", operation, message, data, correlationId);
        await PublishAsync("error", log);
        _logger.LogError(ex, "[LOG][ERROR] {Op} - {Msg}", operation, message);
    }

    public async Task PublishFailoverAsync(string operation, string message, Dictionary<string, object>? data = null, string? correlationId = null)
    {
        var log = CreateBaseLog("failover", "system", operation, message, data, correlationId);
        await PublishAsync("failover", log);
        _logger.LogWarning("[LOG][FAILOVER] {Op} - {Msg}", operation, message);
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================
    private LogMessage CreateBaseLog(string type, string category, string operation, string message, Dictionary<string, object>? data, string? correlationId)
        => new()
        {
            Layer = "Traffic",
            Level = "Edge",
            Service = "IntersectionController",
            Domain = "[CONTROLLER][INTERSECTION]",
            Type = type,
            Category = category,
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
        var routingKey = _routingPattern.Replace("*", type);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}