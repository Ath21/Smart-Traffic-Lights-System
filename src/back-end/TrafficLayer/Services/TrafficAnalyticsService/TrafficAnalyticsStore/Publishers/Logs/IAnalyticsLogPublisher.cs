using LogMessages;

namespace TrafficAnalyticsStore.Publishers.Logs;

public interface IAnalyticsLogPublisher
{
    // log.traffic.analytics_service.audit
    Task PublishAuditAsync(AuditLogMessage message);

    // log.traffic.analytics_service.error
    Task PublishErrorAsync(ErrorLogMessage message);
}