using Messages.Traffic;
namespace TrafficLightCoordinatorStore.Engine;

public class DecisionEngine : IDecisionEngine
{
    private readonly ILogger<DecisionEngine> _logger;

    public DecisionEngine(ILogger<DecisionEngine> logger)
    {
        _logger = logger;
    }

    // ============================================================
    // Evaluate Sensor-based Priority Counts
    // ============================================================
    public string EvaluatePriorityCount(PriorityCountMessage message)
    {
        if (message is null) return "Standard";

        _logger.LogInformation("[ENGINE][COUNT] Evaluating {Type} count {Count} (Priority={Level}, ThresholdExceeded={Exceeded})",
            message.CountType, message.TotalCount, message.PriorityLevel, message.IsThresholdExceeded);

        if (message.IsThresholdExceeded)
        {
            return message.CountType?.ToLowerInvariant() switch
            {
                "vehicle"    => "Peak",
                "pedestrian" => "Pedestrian",
                "cyclist"    => "Cyclist",
                _            => "Standard"
            };
        }

        return "Standard";
    }

    // ============================================================
    // Evaluate Priority Event Messages
    // ============================================================
    public string EvaluatePriorityEvent(PriorityEventMessage message)
    {
        if (message is null) return "Standard";

        _logger.LogInformation("[ENGINE][EVENT] Evaluating event {EventType} (VehicleType={VehicleType}, Dir={Dir}, Priority={Level})",
            message.EventType, message.VehicleType, message.Direction, message.PriorityLevel);

        return message.EventType?.ToLowerInvariant() switch
        {
            "emergency vehicle" => "Emergency",
            "public transport"  => "PublicTransport",
            "incident"          => "Incident",
            _                   => "Standard"
        };
    }

    // ============================================================
    // Evaluate Traffic Analytics Metrics
    // ============================================================
    public string EvaluateTrafficAnalytics(TrafficAnalyticsMessage message)
    {
        if (message is null) return "Standard";

        _logger.LogInformation("[ENGINE][ANALYTICS] Evaluating metric {Type} for {Intersection} | Speed={Speed:F1} km/h, Wait={Wait:F1}s, Congestion={Cong:F2}",
            message.MetricType, message.IntersectionName, message.AverageSpeedKmh, message.AverageWaitTimeSec, message.CongestionIndex);

        return message.MetricType?.ToLowerInvariant() switch
        {
            "incident"   => "Incident",
            "congestion" => message.CongestionIndex > 0.7 ? "Peak" : "Standard",
            "summary"    => "Standard",
            _            => "Standard"
        };
    }
}
