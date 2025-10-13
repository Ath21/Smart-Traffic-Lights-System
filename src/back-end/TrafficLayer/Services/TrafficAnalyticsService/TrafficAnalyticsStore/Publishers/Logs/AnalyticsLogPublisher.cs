using LogMessages;
using MassTransit;

namespace TrafficAnalyticsStore.Publishers.Logs;

public class AnalyticsLogPublisher : IAnalyticsLogPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<AnalyticsLogPublisher> _logger;
    private readonly string _auditKey;
    private readonly string _errorKey;

    private const string ServiceTag = "[" + nameof(AnalyticsLogPublisher) + "]";

    public AnalyticsLogPublisher(IConfiguration configuration, ILogger<AnalyticsLogPublisher> logger, IBus bus)
    {
        _bus = bus;
        _logger = logger;

        _auditKey = configuration["RabbitMQ:RoutingKeys:Log:Audit"] 
                    ?? "log.traffic.analytics_service.audit";

        _errorKey = configuration["RabbitMQ:RoutingKeys:Log:Error"] 
                    ?? "log.traffic.analytics_service.error";
    }

    public async Task PublishAuditAsync(AuditLogMessage message)
    {
        await _bus.Publish(message, ctx => ctx.SetRoutingKey(_auditKey));

        _logger.LogInformation(
            "{Tag} Published Audit Log {LogId} for Service {Service}: {Action} - {Details}",
            ServiceTag, message.LogId, message.ServiceName, message.Action, message.Details
        );
    }

    public async Task PublishErrorAsync(ErrorLogMessage message)
    {
        await _bus.Publish(message, ctx => ctx.SetRoutingKey(_errorKey));

        _logger.LogError(
            "{Tag} Published Error Log {LogId} for Service {Service}: {ErrorType} - {Message}",
            ServiceTag, message.LogId, message.ServiceName, message.ErrorType, message.Message
        );
    }
}
