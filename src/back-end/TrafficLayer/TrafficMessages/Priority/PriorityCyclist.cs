namespace TrafficMessages.Priority;

public record PriorityCyclist(
    string IntersectionId,
    bool PriorityActive,
    DateTime Timestamp
);