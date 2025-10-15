using System;
using Messages.Sensor;
using TrafficAnalyticsStore.Models;

namespace TrafficAnalyticsStore.Aggregators;

public interface ITrafficAnalyticsAggregator
{
    Task UpdateCountAsync(SensorCountMessage msg);
    Task UpdateEventAsync(DetectionEventMessage msg);
    Task CreateAndPublishAlertAsync(
        DailySummaryDto summary,
        string type,
        double congestionIndex,
        int severity,
        string? customMessage = null);
}
