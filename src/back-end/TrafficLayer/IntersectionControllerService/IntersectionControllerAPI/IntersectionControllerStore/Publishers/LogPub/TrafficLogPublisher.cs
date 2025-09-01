using System;
using System.Threading.Tasks;
using LogMessages;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IntersectionControlStore.Publishers.LogPub;

public class TrafficLogPublisher : ITrafficLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLogPublisher> _logger;
    private readonly string _logExchange;
    private readonly string _auditRoutingKey;
    private readonly string _errorRoutingKey;

    private const string ServiceTag = "[" + nameof(TrafficLogPublisher) + "]";

    public TrafficLogPublisher(IBus bus, ILogger<TrafficLogPublisher> logger, IConfiguration configuration)
    {
        _bus = bus;
        _logger = logger;

        _logExchange = configuration["RabbitMQ:Exchanges:Logs"] ?? "LOG.EXCHANGE";
        _auditRoutingKey = configuration["RabbitMQ:RoutingKeys:LogsAudit"] ?? "log.intersection_controller.audit";
        _errorRoutingKey = configuration["RabbitMQ:RoutingKeys:LogsError"] ?? "log.intersection_controller.error";
    }

    public async Task PublishAuditAsync(string serviceName, string action, string details, object? metadata = null, string? intersectionId = null)
    {
        var log = new AuditLogMessage(Guid.NewGuid(), serviceName, action, details, DateTime.UtcNow, metadata);

        _logger.LogInformation("{Tag} Publishing AUDIT log for {Service}: {Action} - {Details}",
            ServiceTag, serviceName, action, details);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(_auditRoutingKey));

        _logger.LogInformation("{Tag} AUDIT log published successfully", ServiceTag);
    }

    public async Task PublishErrorAsync(string serviceName, string errorType, string message, object? metadata = null, string? intersectionId = null)
    {
        var log = new ErrorLogMessage(Guid.NewGuid(), serviceName, errorType, message, DateTime.UtcNow, metadata);

        _logger.LogError("{Tag} Publishing ERROR log for {Service}: {Type} - {Message}",
            ServiceTag, serviceName, errorType, message);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logExchange}"));
        await endpoint.Send(log, ctx => ctx.SetRoutingKey(_errorRoutingKey));

        _logger.LogInformation("{Tag} ERROR log published successfully", ServiceTag);
    }
}