using MassTransit;
using Messages.Traffic;

namespace TrafficAnalyticsService.Publishers.Metrics;

public class TrafficAnalyticsMetricsPublisher : ITrafficAnalyticsMetricsPublisher
{
    private readonly IBus _bus;
    private readonly ILogger<TrafficAnalyticsMetricsPublisher> _logger;
    private readonly IntersectionContext _intersection;
    private readonly string _routingPattern;

    public TrafficAnalyticsMetricsPublisher(
        IConfiguration config,
        ILogger<TrafficAnalyticsMetricsPublisher> logger,
        IBus bus,
        IntersectionContext intersection)
    {
        _bus = bus;
        _logger = logger;
        _intersection = intersection;

        _routingPattern = config["RabbitMQ:RoutingKeys:Traffic:Analytics"]
                          ?? "traffic.analytics.{intersection}.{metric}";
    }

    // ============================================================
    // PUBLISH SUMMARY METRIC
    // ============================================================
    public async Task PublishSummaryAsync(
        double avgSpeed, double avgWait, int vehicleCount, int pedCount, int cycCount,
        double congestionIndex, Guid? correlationId = null)
    {
        var msg = CreateBaseMetric("Summary", avgSpeed, avgWait, vehicleCount, pedCount, cycCount, congestionIndex, 0, correlationId);
        await PublishAsync("summary", msg);
        _logger.LogInformation("[{Intersection}] SUMMARY analytics published (Speed={Speed:F1}, Wait={Wait:F1}, Index={Index:F2})",
            _intersection.Name, avgSpeed, avgWait, congestionIndex);
    }

    // ============================================================
    // PUBLISH CONGESTION METRIC
    // ============================================================
    public async Task PublishCongestionAsync(
        double avgSpeed, double avgWait, int vehicleCount, int pedCount, int cycCount,
        double congestionIndex, int severity, Guid? correlationId = null)
    {
        var msg = CreateBaseMetric("Congestion", avgSpeed, avgWait, vehicleCount, pedCount, cycCount, congestionIndex, severity, correlationId);
        await PublishAsync("congestion", msg);
        _logger.LogWarning("[{Intersection}] CONGESTION analytics published (Speed={Speed:F1}, Wait={Wait:F1}, Index={Index:F2}, Severity={Severity})",
            _intersection.Name, avgSpeed, avgWait, congestionIndex, severity);
    }

    // ============================================================
    // PUBLISH INCIDENT METRIC
    // ============================================================
    public async Task PublishIncidentAsync(
        double avgSpeed, double avgWait, int vehicleCount, int pedCount, int cycCount,
        double congestionIndex, int severity, Guid? correlationId = null)
    {
        var msg = CreateBaseMetric("Incident", avgSpeed, avgWait, vehicleCount, pedCount, cycCount, congestionIndex, severity, correlationId);
        await PublishAsync("incident", msg);
        _logger.LogError("[{Intersection}] INCIDENT analytics published (Severity={Severity}, Index={Index:F2})",
            _intersection.Name, severity, congestionIndex);
    }

    // ============================================================
    // Internal helpers
    // ============================================================
    private TrafficAnalyticsMessage CreateBaseMetric(
        string metricType,
        double avgSpeed,
        double avgWait,
        int vehicleCount,
        int pedCount,
        int cycCount,
        double congestionIndex,
        int severity,
        Guid? correlationId)
    {
        return new TrafficAnalyticsMessage
        {
            CorrelationId = correlationId ?? Guid.Empty,
            IntersectionId = _intersection.Id,
            IntersectionName = _intersection.Name,
            MetricType = metricType,
            AverageSpeedKmh = avgSpeed,
            AverageWaitTimeSec = avgWait,
            TotalVehicleCount = vehicleCount,
            TotalPedestrianCount = pedCount,
            TotalCyclistCount = cycCount,
            CongestionIndex = congestionIndex,
            Severity = severity,
            SourceServices = new() { "Traffic Analytics Service" },
            DestinationServices = new()
            {
                "Traffic Light Coordinator Service",
                "Notification Service",
                "User Service"
            }
        };
    }

    private async Task PublishAsync(string metricKey, TrafficAnalyticsMessage msg)
    {
        msg.CorrelationId = msg.CorrelationId == Guid.Empty ? Guid.NewGuid() : msg.CorrelationId;
        msg.Timestamp = DateTime.UtcNow;

        var routingKey = _routingPattern
            .Replace("{intersection}", _intersection.Name.ToLower().Replace(' ', '-'))
            .Replace("{metric}", metricKey);

        await _bus.Publish(msg, ctx => ctx.SetRoutingKey(routingKey));
    }
}
