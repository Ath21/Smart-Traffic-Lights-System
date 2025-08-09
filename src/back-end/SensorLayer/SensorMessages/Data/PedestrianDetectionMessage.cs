namespace SensorMessages.Data;

public record PedestrianDetectionMessage(
    Guid IntersectionId,
    int PedestrianCount,
    DateTime Timestamp
);