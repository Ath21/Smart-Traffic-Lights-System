using Messages.Sensor;
using Microsoft.Extensions.Logging;
using TrafficAnalyticsStore.Business.Alerts;
using TrafficAnalyticsStore.Business.DailySummary;
using TrafficAnalyticsStore.Models;
using TrafficAnalyticsStore.Publishers.Analytics;

namespace TrafficAnalyticsStore.Aggregators;

public class TrafficAnalyticsAggregator : ITrafficAnalyticsAggregator
{
    private readonly IDailySummaryBusiness _summaryService;
    private readonly IAlertBusiness _alertService;
    private readonly ITrafficAnalyticsPublisher _publisher;
    private readonly ILogger<TrafficAnalyticsAggregator> _logger;

    public TrafficAnalyticsAggregator(
        IDailySummaryBusiness summaryService,
        IAlertBusiness alertService,
        ITrafficAnalyticsPublisher publisher,
        ILogger<TrafficAnalyticsAggregator> logger)
    {
        _summaryService = summaryService;
        _alertService = alertService;
        _publisher = publisher;
        _logger = logger;
    }

    // ============================================================
    // SENSOR COUNT HANDLER
    // ============================================================
    public async Task UpdateCountAsync(SensorCountMessage msg)
    {
        var summary = await _summaryService.GetOrCreateTodayAsync(msg.IntersectionId, msg.IntersectionName);
        await _summaryService.UpdateCountsAsync(summary, msg.CountType, msg.Count, msg.AverageSpeedKmh, msg.AverageWaitTimeSec);

        // Recalculate congestion index
        double ci = summary.CongestionIndex;
        int severity = ci switch
        {
            >= 0.9 => 5,
            >= 0.8 => 4,
            >= 0.7 => 3,
            >= 0.5 => 2,
            _ => 1
        };

        if (ci > 0.7)
        {
            await CreateAndPublishAlertAsync(summary, "Congestion", ci, severity);
        }
        else
        {
            await _publisher.PublishSummaryAsync(
                summary.IntersectionId,
                summary.Intersection,
                summary.AverageSpeedKmh,
                summary.AverageWaitTimeSec,
                summary.TotalVehicles,
                summary.TotalPedestrians,
                summary.TotalCyclists,
                summary.CongestionIndex);
        }

        _logger.LogInformation(
            "[ANALYTICS][SUMMARY] {Intersection}: Count={Vehicles} Veh, Speed={Speed:F1} km/h, Wait={Wait:F1}s, CI={CI:F2}",
            summary.Intersection,
            summary.TotalVehicles,
            summary.AverageSpeedKmh,
            summary.AverageWaitTimeSec,
            summary.CongestionIndex);
    }

    // ============================================================
    // DETECTION EVENT HANDLER
    // ============================================================
    public async Task UpdateEventAsync(DetectionEventMessage msg)
    {
        if (!string.Equals(msg.EventType, "Incident", StringComparison.OrdinalIgnoreCase))
            return;

        var model = new DailySummaryDto
        {
            IntersectionId = msg.IntersectionId,
            Intersection = msg.IntersectionName,
            AverageSpeedKmh = 0,
            AverageWaitTimeSec = 0,
            TotalVehicles = 0
        };

        await CreateAndPublishAlertAsync(
            model,
            "Incident",
            congestionIndex: 0.9,
            severity: 5,
            customMessage: $"Incident detected at {msg.IntersectionName} ({msg.Direction} direction).");
    }

    // ============================================================
    // INTERNAL HELPERS
    // ============================================================
    private async Task CreateAndPublishAlertAsync(
        DailySummaryDto summary,
        string type,
        double congestionIndex,
        int severity,
        string? customMessage = null)
    {
        string message = customMessage ?? $"{type} detected at {summary.Intersection}.";
        var alert = await _alertService.CreateAlertAsync(
            summary.IntersectionId,
            summary.Intersection,
            type,
            message,
            congestionIndex,
            severity);

        _logger.LogWarning(
            "[ANALYTICS][ALERT] {Type} at {Intersection} (Severity={Severity}, CI={CI:F2}) → {Message}",
            type, summary.Intersection, severity, congestionIndex, message);

        if (type == "Incident")
        {
            await _publisher.PublishIncidentAsync(
                summary.IntersectionId,
                summary.Intersection,
                summary.AverageSpeedKmh,
                summary.AverageWaitTimeSec,
                summary.TotalVehicles,
                summary.TotalPedestrians,
                summary.TotalCyclists,
                congestionIndex,
                severity,
                alert.Message);
        }
        else if (type == "Congestion")
        {
            await _publisher.PublishCongestionAsync(
                summary.IntersectionId,
                summary.Intersection,
                summary.AverageSpeedKmh,
                summary.AverageWaitTimeSec,
                summary.TotalVehicles,
                summary.TotalPedestrians,
                summary.TotalCyclists,
                congestionIndex,
                severity,
                alert.Message);
        }
    }
}
