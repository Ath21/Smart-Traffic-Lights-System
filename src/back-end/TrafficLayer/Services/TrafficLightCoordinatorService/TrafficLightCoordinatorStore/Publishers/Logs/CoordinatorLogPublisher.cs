using System;
using MassTransit;
using Messages.Log;

namespace TrafficLightCoordinatorStore.Publishers.Logs;

public class CoordinatorLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<CoordinatorLogPublisher> _logger;
    private readonly string _routingPattern;

    public CoordinatorLogPublisher(
        IBus bus,
        IConfiguration config,
        ILogger<CoordinatorLogPublisher> logger)
    {
        _bus = bus;
        _logger = logger;

        _routingPattern = config["RabbitMQ:RoutingKeys:Log:Coordinator"]
                          ?? "log.sensor.traffic-light-coordinator-service.{type}";
    }

    public async Task PublishAuditAsync(
        string action,
        string message,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        await PublishAsync("audit", new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Traffic Layer",
            DestinationLayer = new () { "Log Layer" },

            SourceService = "Traffic Light Coordinator Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Audit",

            Action = action,
            Message = message,

            Metadata = metadata
        });

        _logger.LogInformation("[PUBLISHER][LOG] AUDIT log published (Action={Action}) - Message={Message}", action, message);
    }

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
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Traffic Layer",
            DestinationLayer = new () { "Log Layer" },

            SourceService = "Traffic Light Coordinator Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Error",

            Action = action,
            Message = message,

            Metadata = metadata
        });

        _logger.LogError("[PUBLISHER][LOG] ERROR log published (Action={Action}) - Message={Message}", action, message);
    }

    public async Task PublishFailoverAsync(
        string action,
        string message,
        Dictionary<string, string>? metadata = null,
        Guid? correlationId = null)
    {
        await PublishAsync("failover", new LogMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Traffic Layer",
            DestinationLayer = new () { "Log Layer" },

            SourceService = "Traffic Light Coordinator Service",
            DestinationServices = new() { "Log Service" },

            LogType = "Failover",

            Action = action,
            Message = message,

            Metadata = metadata
        });

        _logger.LogWarning("[PUBLISHER][LOG] FAILOVER log published (Action={Action} - Message={Message})", action, message);
    }

    private async Task PublishAsync(string logType, LogMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern.Replace("{type}", logType);
        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
