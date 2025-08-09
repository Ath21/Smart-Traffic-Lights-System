namespace SensorMessages.Data;

public record VehicleCountMessage(
    Guid IntersectionId,
    int VehicleCount,
    double AvgSpeed,
    DateTime Timestamp
);