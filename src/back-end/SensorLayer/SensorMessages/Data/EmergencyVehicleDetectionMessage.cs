namespace SensorMessages.Data;

public record EmergencyVehicleDetectionMessage(
    Guid IntersectionId,
    string VehicleId,
    string VehicleType, // e.g., ambulance, fire truck
    double Speed,
    DateTime Timestamp
);
