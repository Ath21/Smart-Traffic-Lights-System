namespace TrafficMessages.Priority;

public record PriorityPublicTransport(
    string IntersectionId,
    bool PriorityActive,
    DateTime Timestamp
);
