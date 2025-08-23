namespace TrafficMessages;

// traffic.analytics.incident.<intersection_id>
public record TrafficIncidentMessage(
    Guid IncidentId,
    Guid IntersectionId,
    string Description,
    DateTime Timestamp
);