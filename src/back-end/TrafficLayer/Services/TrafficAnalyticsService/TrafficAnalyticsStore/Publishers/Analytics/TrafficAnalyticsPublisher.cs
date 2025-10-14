using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Messages.Traffic;

namespace TrafficAnalyticsStore.Publishers.Analytics;

public class TrafficAnalyticsPublisher : ITrafficAnalyticsPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficAnalyticsPublisher> _logger;
    private readonly string _routingPattern;

    public TrafficAnalyticsPublisher(
        IConfiguration config,
        ILogger<TrafficAnalyticsPublisher> logger,
        IBus bus)
    {
        _bus = bus;
        _logger = logger;
        _routingPattern = config["RabbitMQ:RoutingKeys:Analytics"]
                          ?? "traffic.analytics.{intersection}.{metric}";
    }

    // ============================================================
    // SUMMARY METRIC
    // ============================================================
    public async Task PublishSummaryAsync(
        int intersectionId,
        string intersectionName,
        double avgSpeed,
        double avgWait,
        int vehicleCount,
        int pedestrianCount,
        int cyclistCount,
        double congestionIndex,
        Guid? correlationId = null,
        Dictionary<string, string>? metadata = null)
    {
        var msg = CreateBaseMessage("Summary", correlationId, metadata);

        msg.IntersectionId = intersectionId;
        msg.IntersectionName = intersectionName;

        msg.AverageSpeedKmh = avgSpeed;
        msg.AverageWaitTimeSec = avgWait;
        msg.TotalVehicleCount = vehicleCount;
        msg.TotalPedestrianCount = pedestrianCount;
        msg.TotalCyclistCount = cyclistCount;
        msg.CongestionIndex = congestionIndex;
        msg.Severity = 0;

        await PublishAsync(intersectionName, "summary", msg);

        _logger.LogInformation(
            "[PUBLISHER][ANALYTICS][{Intersection}] SUMMARY metric published (Speed={Speed:F1} km/h, Wait={Wait:F1}s, CI={CI:F2})",
            intersectionName, avgSpeed, avgWait, congestionIndex);
    }

    // ============================================================
    // CONGESTION METRIC (alert-level)
    // ============================================================
    public async Task PublishCongestionAsync(
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
        Dictionary<string, string>? metadata = null)
    {
        var msg = CreateBaseMessage("Congestion", correlationId, metadata);

        msg.IntersectionId = intersectionId;
        msg.IntersectionName = intersectionName;

        msg.AverageSpeedKmh = avgSpeed;
        msg.AverageWaitTimeSec = avgWait;
        msg.TotalVehicleCount = vehicleCount;
        msg.TotalPedestrianCount = pedestrianCount;
        msg.TotalCyclistCount = cyclistCount;
        msg.CongestionIndex = congestionIndex;
        msg.Severity = severity;

        msg.Metadata ??= new();
        if (!string.IsNullOrWhiteSpace(alertMessage))
            msg.Metadata["alert"] = alertMessage;

        await PublishAsync(intersectionName, "congestion", msg);

        _logger.LogWarning(
            "[PUBLISHER][ANALYTICS][{Intersection}] CONGESTION ALERT published (CI={CI:F2}, Sev={Severity})",
            intersectionName, congestionIndex, severity);
    }

    // ============================================================
    // INCIDENT METRIC (alert-level)
    // ============================================================
    public async Task PublishIncidentAsync(
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
        Dictionary<string, string>? metadata = null)
    {
        var msg = CreateBaseMessage("Incident", correlationId, metadata);

        msg.IntersectionId = intersectionId;
        msg.IntersectionName = intersectionName;

        msg.AverageSpeedKmh = avgSpeed;
        msg.AverageWaitTimeSec = avgWait;
        msg.TotalVehicleCount = vehicleCount;
        msg.TotalPedestrianCount = pedestrianCount;
        msg.TotalCyclistCount = cyclistCount;
        msg.CongestionIndex = congestionIndex;
        msg.Severity = severity;

        msg.Metadata ??= new();
        if (!string.IsNullOrWhiteSpace(alertMessage))
            msg.Metadata["alert"] = alertMessage;

        await PublishAsync(intersectionName, "incident", msg);

        _logger.LogError(
            "[PUBLISHER][ANALYTICS][{Intersection}] INCIDENT ALERT published (CI={CI:F2}, Sev={Severity})",
            intersectionName, congestionIndex, severity);
    }

    // ============================================================
    // Internal helpers
    // ============================================================
    private TrafficAnalyticsMessage CreateBaseMessage(
        string metricType,
        Guid? correlationId,
        Dictionary<string, string>? metadata)
    {
        return new TrafficAnalyticsMessage
        {
            CorrelationId = correlationId ?? Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,

            SourceLayer = "Traffic Layer",
            DestinationLayer = new() { "Traffic Layer", "User Layer" },

            SourceService = "Traffic Analytics Service",
            DestinationServices = new()
            {
                "Traffic Light Coordinator Service",
                "User Service",
                "Notification Service"
            },

            MetricType = metricType,
            Metadata = metadata
        };
    }

    private async Task PublishAsync(string intersectionName, string metricKey, TrafficAnalyticsMessage msg)
    {
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern
            .Replace("{intersection}", intersectionName.ToLower().Replace(' ', '-'))
            .Replace("{metric}", metricKey);

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
