namespace TrafficMessages;

// priority.*.{intersection}
public record PriorityMessage(
    string Intersection,   // e.g. "ekklhsia"
    string PriorityType,   // emergency, public_transport, cyclist, pedestrian, incident
    string? DetectionId,   // optional string (sensor id, camera id, etc.)
    string? Reason,
    DateTime Timestamp
);
