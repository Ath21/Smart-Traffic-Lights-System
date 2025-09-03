using System;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LogMessages;

namespace TrafficLightControlStore.Publishers.Logs;

public class TrafficLogPublisher : ITrafficLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficLogPublisher> _logger;
    private readonly IConfiguration _configuration;

    private readonly string _logsExchange;
    private readonly string _auditRoutingKey;
    private readonly string _errorRoutingKey;

    private const string ServiceTag = "[TrafficLogPublisher]";

    public TrafficLogPublisher(
        IBus bus,
        ILogger<TrafficLogPublisher> logger,
        IConfiguration configuration)
    {
        _bus = bus;
        _logger = logger;
        _configuration = configuration;

        // Extract from docker-compose env
        _logsExchange    = _configuration["RabbitMQ:Exchanges:Logs"] 
                           ?? "LOG.EXCHANGE";

        _auditRoutingKey = _configuration["RabbitMQ:RoutingKeys:TrafficLogsAudit"] 
                           ?? "log.traffic.light_service.audit";

        _errorRoutingKey = _configuration["RabbitMQ:RoutingKeys:TrafficLogsError"] 
                           ?? "log.traffic.light_service.error";
    }

    public async Task PublishAuditLogAsync(string serviceName, string action, string details, object? metadata = null)
    {
        var logMessage = new AuditLogMessage(
            Guid.NewGuid(),
            serviceName,
            action,
            details,
            DateTime.UtcNow,
            metadata
        );

        _logger.LogInformation("{Tag} Publishing AUDIT log: {Action} - {Details}", ServiceTag, action, details);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logsExchange}"));
        await endpoint.Send(logMessage, ctx => ctx.SetRoutingKey(_auditRoutingKey));

        _logger.LogInformation("{Tag} Audit log published to {Exchange} with {RoutingKey}",
            ServiceTag, _logsExchange, _auditRoutingKey);
    }

    public async Task PublishErrorLogAsync(string serviceName, string errorType, string message, object? metadata = null)
    {
        var logMessage = new ErrorLogMessage(
            Guid.NewGuid(),
            serviceName,
            errorType,
            message,
            DateTime.UtcNow,
            metadata
        );

        _logger.LogInformation("{Tag} Publishing ERROR log: {ErrorType} - {Message}", ServiceTag, errorType, message);

        var endpoint = await _bus.GetSendEndpoint(new Uri($"exchange:{_logsExchange}"));
        await endpoint.Send(logMessage, ctx => ctx.SetRoutingKey(_errorRoutingKey));

        _logger.LogInformation("{Tag} Error log published to {Exchange} with {RoutingKey}",
            ServiceTag, _logsExchange, _errorRoutingKey);
    }
}
