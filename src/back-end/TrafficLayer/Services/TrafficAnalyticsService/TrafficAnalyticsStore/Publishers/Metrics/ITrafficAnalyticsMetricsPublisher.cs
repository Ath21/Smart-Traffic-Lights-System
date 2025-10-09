using System;

namespace TrafficAnalyticsStore.Publishers.Metrics;

public interface ITrafficAnalyticsMetricsPublisher
{
    Task PublishSummaryAsync(double avgSpeed, double avgWait, int vehicleCount, int pedCount, int cycCount, double congestionIndex, Guid? correlationId = null);
    Task PublishCongestionAsync(double avgSpeed, double avgWait, int vehicleCount, int pedCount, int cycCount, double congestionIndex, int severity, Guid? correlationId = null);
    Task PublishIncidentAsync(double avgSpeed, double avgWait, int vehicleCount, int pedCount, int cycCount, double congestionIndex, int severity, Guid? correlationId = null);
}