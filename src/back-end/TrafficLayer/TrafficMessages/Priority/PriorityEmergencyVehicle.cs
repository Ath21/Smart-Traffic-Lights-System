namespace TrafficMessages.Priority;

public record PriorityEmergencyVehicle(
    string IntersectionId,
    bool PriorityActive,
    DateTime Timestamp
);
