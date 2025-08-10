namespace TrafficMessages.Priority;

public record PriorityPedestrian(
    string IntersectionId,
    bool PriorityActive,
    DateTime Timestamp
);
