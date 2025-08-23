namespace TrafficMessages;

// priority.*.<intersection_id>
public record PriorityMessage(
    Guid IntersectionId,
    string PriorityType,    // Emergency, PublicTransport, Pedestrian, Cyclist, Incident
    Guid? DetectionId,      // source detection
    string? Reason,
    DateTime Timestamp
);
