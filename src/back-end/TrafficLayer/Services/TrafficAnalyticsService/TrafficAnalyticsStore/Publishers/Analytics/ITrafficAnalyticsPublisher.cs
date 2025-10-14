using System;

namespace TrafficAnalyticsStore.Publishers.Analytics;

public interface ITrafficAnalyticsPublisher
{
    Task PublishSummaryAsync(
        int intersectionId,
        string intersectionName,
        double avgSpeed,
        double avgWait,
        int vehicleCount,
        int pedestrianCount,
        int cyclistCount,
        double congestionIndex,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);

    Task PublishCongestionAsync(
        int intersectionId,
        string intersectionName,
        double avgSpeed,
        double avgWait,
        int vehicleCount,
        int pedestrianCount,
        int cyclistCount,
        double congestionIndex,
        int severity,
        string? alertMessage = null,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);

    Task PublishIncidentAsync(
        int intersectionId,
        string intersectionName,
        double avgSpeed,
        double avgWait,
        int vehicleCount,
        int pedestrianCount,
        int cyclistCount,
        double congestionIndex,
        int severity,
        string? alertMessage = null,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null);
}

