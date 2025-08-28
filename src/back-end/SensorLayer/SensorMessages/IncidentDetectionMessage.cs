namespace SensorMessages;

// sensor.incident.detected.{intersection_id}
public record IncidentDetectionMessage(
    Guid DetectionId,
    Guid IntersectionId,
    string Description,
    DateTime Timestamp
);