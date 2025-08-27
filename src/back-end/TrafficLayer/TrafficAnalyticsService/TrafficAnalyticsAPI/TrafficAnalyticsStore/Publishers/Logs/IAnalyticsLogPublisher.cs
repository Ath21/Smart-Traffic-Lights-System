using System;
using LogMessages;

namespace TrafficAnalyticsStore.Publishers.Logs;

public interface IAnalyticsLogPublisher
{
    Task PublishAuditAsync(AuditLogMessage message);
    Task PublishErrorAsync(ErrorLogMessage message);
}