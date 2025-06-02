namespace LogStore.Messages.Traffic;

public record class TrafficCongestionAlert(
    string IntersectionId,
    double CongestionLevel,
    string Severity,
    string Description,
    DateTime Timestamp
);