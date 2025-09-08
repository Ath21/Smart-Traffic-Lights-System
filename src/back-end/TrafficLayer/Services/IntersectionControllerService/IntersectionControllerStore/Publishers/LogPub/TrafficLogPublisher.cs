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
    private readonly string _auditKey;
    private readonly string _errorKey;

    private const string ServiceTag = "[" + nameof(TrafficLogPublisher) + "]";

    public TrafficLogPublisher(IBus bus, ILogger<TrafficLogPublisher> logger, IConfiguration configuration)
    {
        _bus = bus;
        _logger = logger;

        _logExchange = configuration["RabbitMQ:Exchanges:Log"] ?? "LOG.EXCHANGE";
        _auditKey    = configuration["RabbitMQ:RoutingKeys:Log:Audit"] 
                       ?? "log.traffic.intersection_controller_service.audit";
        _errorKey    = configuration["RabbitMQ:RoutingKeys:Log:Error"] 
                       ?? "log.traffic.intersection_controller_service.error";
    }

    public async Task PublishAuditAsync(string serviceName, string action, string details, object? metadata = null, string? intersectionId = null)
    {
        var log = new AuditLogMessage(Guid.NewGuid(), serviceName, action, details, DateTime.UtcNow, metadata);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(_auditKey));

        _logger.LogInformation("{Tag} AUDIT published: {Action} -> {Details}", ServiceTag, action, details);
    }

    public async Task PublishErrorAsync(string serviceName, string errorType, string message, object? metadata = null, string? intersectionId = null)
    {
        var log = new ErrorLogMessage(Guid.NewGuid(), serviceName, errorType, message, DateTime.UtcNow, metadata);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(_errorKey));

        _logger.LogError("{Tag} ERROR published: {Type} -> {Message}", ServiceTag, errorType, message);
    }
}
