namespace SensorMessages.Data;

public record CyclistDetectionMessage(
    Guid IntersectionId,
    int CyclistCount,
    double AvgSpeed,
    DateTime Timestamp
);
