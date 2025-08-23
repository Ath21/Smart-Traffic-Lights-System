namespace SensorMessages;

// sensor.public_transport.request.<intersection_id>
public record PublicTransportMessage(
    Guid DetectionId,
    Guid IntersectionId,
    string? RouteId,     // optional
    DateTime Timestamp
);