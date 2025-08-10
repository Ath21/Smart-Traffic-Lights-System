namespace TrafficMessages.Analytics;

public record TrafficCongestionAlert(
    string IntersectionId,
    double CongestionLevel,
    string Severity,
    string Description,
    DateTime Timestamp
);