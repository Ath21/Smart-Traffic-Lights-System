namespace SensorMessages;

// sensor.public_transport.request.{intersection_id}
public record PublicTransportDetectionMessage(
    Guid DetectionId,
    Guid IntersectionId,
    string? RouteId,     
    DateTime Timestamp
);