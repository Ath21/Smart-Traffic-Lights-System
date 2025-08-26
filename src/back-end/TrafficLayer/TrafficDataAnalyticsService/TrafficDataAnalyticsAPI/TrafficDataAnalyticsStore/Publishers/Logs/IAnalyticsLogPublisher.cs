using System;
using LogMessages;

namespace TrafficDataAnalyticsStore.Publishers.Logs;

public interface IAnalyticsLogPublisher
{
    Task PublishAuditAsync(AuditLogMessage message);
    Task PublishErrorAsync(ErrorLogMessage message);
}