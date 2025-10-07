// traffic.analytics.{intersection}.{metric}
//
// {intersection} : agiou-spyridonos, anatoliki-pyli, dytiki-pyli, ekklisia, kentriki-pyli
// {metric}       : incident, congestion, summary
//
// Published by : Traffic Analytics Service
// Consumed by  : Traffic Light Coordinator Service, User Service, Notification Service
public class TrafficAnalyticsMetricMessage : BaseMessage
{
    public string MetricType { get; set; }  // Incident, Congestion, Summary

    public double AverageSpeedKmh { get; set; }

    public double AverageWaitTimeSec { get; set; }

    public int TotalVehicleCount { get; set; }

    public int TotalPedestrianCount { get; set; }

    public int TotalCyclistCount { get; set; }

    public double CongestionIndex { get; set; }  // For congestion metrics (0-1 scale)

    public int Severity { get; set; }  // 1 (Low) to 5 (Critical) for incident metrics
}
