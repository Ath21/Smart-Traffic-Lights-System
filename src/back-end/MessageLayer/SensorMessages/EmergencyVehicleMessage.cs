namespace SensorMessages;

// sensor.vehicle.emergency.{intersection_id}
public record EmergencyVehicleMessage(
    Guid DetectionId,
    Guid IntersectionId,
    bool Detected,
    DateTime Timestamp
);