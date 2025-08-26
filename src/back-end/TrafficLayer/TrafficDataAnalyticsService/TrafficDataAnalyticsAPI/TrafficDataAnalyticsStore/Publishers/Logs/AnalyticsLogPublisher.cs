using System;
using LogMessages;
using MassTransit;

namespace TrafficDataAnalyticsStore.Publishers.Logs;

public class AnalyticsLogPublisher : IAnalyticsLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<AnalyticsLogPublisher> _logger;
    private readonly string _auditKey;
    private readonly string _errorKey;

    public AnalyticsLogPublisher(IConfiguration configuration, ILogger<AnalyticsLogPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _auditKey = configuration["RabbitMQ:RoutingKeys:TrafficAuditLog"] 
                    ?? "log.traffic.analytics_service.audit";

        _errorKey = configuration["RabbitMQ:RoutingKeys:TrafficErrorLog"] 
                    ?? "log.traffic.analytics_service.error";
    }

    public async Task PublishAuditAsync(AuditLogMessage message)
    {
        await _bus.Publish(message, ctx => ctx.SetRoutingKey(_auditKey));

        _logger.LogInformation(
            "Published Audit Log {LogId} for Service {Service}: {Action} - {Details}",
            message.LogId, message.ServiceName, message.Action, message.Details
        );
    }

    public async Task PublishErrorAsync(ErrorLogMessage message)
    {
        await _bus.Publish(message, ctx => ctx.SetRoutingKey(_errorKey));

        _logger.LogError(
            "Published Error Log {LogId} for Service {Service}: {ErrorType} - {Message}",
            message.LogId, message.ServiceName, message.ErrorType, message.Message
        );
    }
}
