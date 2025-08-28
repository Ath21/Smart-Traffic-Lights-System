namespace TrafficMessages;

// traffic.analytics.summary.{intersection_id}
public record TrafficSummaryMessage(
    Guid SummaryId,
    Guid IntersectionId,
    DateTime Date,
    float AvgSpeed,
    int VehicleCount,
    string CongestionLevel
);
