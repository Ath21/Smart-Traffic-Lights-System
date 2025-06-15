namespace LogStore.Messages.Traffic;

public record TrafficCongestionAlert(
    string IntersectionId,
    double CongestionLevel,
    string Severity,
    string Description,
    DateTime Timestamp
);