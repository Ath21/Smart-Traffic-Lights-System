namespace SensorMessages;

// sensor.vehicle.count.<intersection_id>
public record VehicleCountMessage(
    Guid DetectionId,
    Guid IntersectionId,
    int VehicleCount,
    float AvgSpeed,
    DateTime Timestamp
);
