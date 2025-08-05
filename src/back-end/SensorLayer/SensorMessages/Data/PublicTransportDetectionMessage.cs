namespace SensorMessages.Data;


public record PublicTransportDetectionMessage(
    Guid IntersectionId,
    string RouteId,
    string VehicleType, // e.g., bus, tram
    int PassengerCount,
    DateTime Timestamp
);