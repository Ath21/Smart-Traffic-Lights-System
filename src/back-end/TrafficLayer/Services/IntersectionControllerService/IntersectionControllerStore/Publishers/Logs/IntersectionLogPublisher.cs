using System;
using MassTransit;
using Messages.Log;

namespace IntersectionControllerStore.Publishers.Logs;

public class IntersectionLogPublisher : IIntersectionLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<IntersectionLogPublisher> _logger;
    private readonly string _routingPattern;

    public IntersectionLogPublisher(
        IBus bus,
        IConfiguration config,
        ILogger<IntersectionLogPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        // Routing: log.sensor.intersection-controller-service.{type}
        _routingPattern = config["RabbitMQ:RoutingKeys:Log:Intersection"]
                          ?? "log.traffic.intersection-controller-service.{type}";
    }

    // ============================================================
    // AUDIT LOG
    // ============================================================
    public async Task PublishAuditAsync(
        string action,
        string message,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        await PublishAsync("audit", new LogMessage
        {
            CorrelationId = correlationId ?? Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Control Layer",
            DestinationLayer = new() { "Log Layer" },

            SourceService = "Intersection Controller Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Audit",
            Action = action,
            Message = message,
            Metadata = metadata
        });

        _logger.LogInformation("[PUBLISHER][LOG][AUDIT] {Action} - {Message}", action, message);
    }

    // ============================================================
    // ERROR LOG
    // ============================================================
    public async Task PublishErrorAsync(
        string action,
        string message,
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

        await PublishAsync("error", new LogMessage
        {
            CorrelationId = correlationId ?? Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Control Layer",
            DestinationLayer = new() { "Log Layer" },

            SourceService = "Intersection Controller Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Error",
            Action = action,
            Message = message,
            Metadata = metadata
        });

        _logger.LogError("[PUBLISHER][LOG][ERROR] {Action} - {Message}", action, message);
    }

    // ============================================================
    // FAILOVER LOG
    // ============================================================
    public async Task PublishFailoverAsync(
        string action,
        string message,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        await PublishAsync("failover", new LogMessage
        {
            CorrelationId = correlationId ?? Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Control Layer",
            DestinationLayer = new() { "Log Layer" },

            SourceService = "Intersection Controller Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Failover",
            Action = action,
            Message = message,
            Metadata = metadata
        });

        _logger.LogWarning("[PUBLISHER][LOG][FAILOVER] {Action} - {Message}", action, message);
    }

    // ============================================================
    // INTERNAL PUBLISH HELPER
    // ============================================================
    private async Task PublishAsync(string type, LogMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern.Replace("{type}", type);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
