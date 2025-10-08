// traffic.analytics.{intersection}.{metric}
//
// {intersection} : agiou-spyridonos, anatoliki-pyli, dytiki-pyli, ekklisia, kentriki-pyli
// {metric}       : incident, congestion, summary
//
// Published by : Traffic Analytics Service
// Consumed by  : Traffic Light Coordinator Service, User Service, Notification Service
public class TrafficAnalyticsMetricMessage : BaseMessage
{
    public string? MetricType { get; set; }  // Incident, Congestion, Summary

    public double AverageSpeedKmh { get; set; }

    public double AverageWaitTimeSec { get; set; }

    public int TotalVehicleCount { get; set; }

    public int TotalPedestrianCount { get; set; }

    public int TotalCyclistCount { get; set; }

    public double CongestionIndex { get; set; }  // For congestion metrics (0-1 scale)

    public int Severity { get; set; }  // 1 (Low) to 5 (Critical) for incident metrics
}

/*

traffic.analytics.agiou-spyridonos.summary

#################
SUMMARY METRIC
#################

{
  "CorrelationId": "a6d54efb-6b34-4c17-b4cd-c04c7f06b3a2",
  "Timestamp": "2025-10-07T14:20:05Z",
  "SourceServices": ["Traffic Analytics Service"],
  "DestinationServices": ["Traffic Light Coordinator Service", "User Service", "Notification Service"],
  "IntersectionId": 2,
  "IntersectionName": "Agiou Spyridonos",
  "MetricType": "Summary",
  "AverageSpeedKmh": 38.7,
  "AverageWaitTimeSec": 12.4,
  "TotalVehicleCount": 1458,
  "TotalPedestrianCount": 242,
  "TotalCyclistCount": 32,
  "CongestionIndex": 0.27,
  "Severity": 0
}

###############
CONGESTION METRIC
###############

traffic.analytics.anatoliki-pyli.congestion

{
  "CorrelationId": "c49c87ef-8a4d-45ad-8e12-b7de4edc3e81",
  "Timestamp": "2025-10-07T14:28:42Z",
  "SourceServices": ["Traffic Analytics Service"],
  "DestinationServices": ["Traffic Light Coordinator Service", "Notification Service", "User Service"],
  "IntersectionId": 4,
  "IntersectionName": "Anatoliki Pyli",
  "MetricType": "Congestion",
  "AverageSpeedKmh": 14.5,
  "AverageWaitTimeSec": 46.8,
  "TotalVehicleCount": 892,
  "TotalPedestrianCount": 76,
  "TotalCyclistCount": 11,
  "CongestionIndex": 0.82,
  "Severity": 4
}

###############
INCIDENT METRIC
###############

traffic.analytics.dytiki-pyli.incident

{
  "CorrelationId": "f91d4bde-1eea-47b8-a41e-45e517f29bc3",
  "Timestamp": "2025-10-07T14:15:05Z",
  "SourceServices": ["Traffic Analytics Service"],
  "DestinationServices": ["Traffic Light Coordinator Service", "Notification Service", "User Service"],
  "IntersectionId": 3,
  "IntersectionName": "Dytiki Pyli",
  "MetricType": "Incident",
  "AverageSpeedKmh": 12.3,
  "AverageWaitTimeSec": 55.8,
  "TotalVehicleCount": 780,
  "TotalPedestrianCount": 96,
  "TotalCyclistCount": 14,
  "CongestionIndex": 0.92,
  "Severity": 5
}


*/
