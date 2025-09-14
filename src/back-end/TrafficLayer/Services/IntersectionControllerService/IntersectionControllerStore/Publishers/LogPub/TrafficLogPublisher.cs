using LogMessages;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IntersectionControllerStore.Publishers.LogPub;

public class TrafficLogPublisher : ITrafficLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLogPublisher> _logger;
    private readonly string _logExchange;

    private readonly string _auditKeyTemplate;
    private readonly string _errorKeyTemplate;
    private readonly string _failoverKeyTemplate;

    private const string ServiceTag = "[" + nameof(TrafficLogPublisher) + "]";

    public TrafficLogPublisher(IBus bus, ILogger<TrafficLogPublisher> logger, IConfiguration configuration)
    {
        _bus = bus;
        _logger = logger;

        _logExchange = configuration["RabbitMQ:Exchanges:Log"] ?? "LOG.EXCHANGE";

        _auditKeyTemplate    = configuration["RabbitMQ:RoutingKeys:Log:Audit"] 
                               ?? "log.traffic.intersection_controller.{intersection}.audit";
        _errorKeyTemplate    = configuration["RabbitMQ:RoutingKeys:Log:Error"] 
                               ?? "log.traffic.intersection_controller.{intersection}.error";
        _failoverKeyTemplate = configuration["RabbitMQ:RoutingKeys:Log:Failover"] 
                               ?? "log.traffic.intersection_controller.{intersection}.failover";
    }

    private static string FormatKey(string template, string? intersectionId) =>
        template.Replace("{intersection}", intersectionId ?? "unknown");

    public async Task PublishAuditAsync(string serviceName, string action, string details, string? intersectionId = null, object? metadata = null)
    {
        var log = new AuditLogMessage(Guid.NewGuid(), serviceName, action, details, DateTime.UtcNow, metadata);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(FormatKey(_auditKeyTemplate, intersectionId)));

        _logger.LogInformation("{Tag} AUDIT published [{Service}] {Action} -> {Details}",
            ServiceTag, serviceName, action, details);
    }

    public async Task PublishErrorAsync(string serviceName, string errorType, string message, string? intersectionId = null, object? metadata = null)
    {
        var log = new ErrorLogMessage(Guid.NewGuid(), serviceName, errorType, message, DateTime.UtcNow, metadata);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(FormatKey(_errorKeyTemplate, intersectionId)));

        _logger.LogError("{Tag} ERROR published [{Service}] {Type} -> {Message}",
            ServiceTag, serviceName, errorType, message);
    }

    public async Task PublishFailoverAsync(string serviceName, string context, string reason, string mode, string? intersectionId = null, object? metadata = null)
    {
        var log = new FailoverMessage(
            LogId: Guid.NewGuid(),
            ServiceName: serviceName,
            Context: context,
            Reason: reason,
            Mode: mode,
            Timestamp: DateTime.UtcNow,
            Metadata: metadata
        );

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(FormatKey(_failoverKeyTemplate, intersectionId)));

        _logger.LogWarning("{Tag} FAILOVER published [{Service}] {Context} -> Mode={Mode}, Reason={Reason}",
            ServiceTag, serviceName, context, mode, reason);
    }
}
