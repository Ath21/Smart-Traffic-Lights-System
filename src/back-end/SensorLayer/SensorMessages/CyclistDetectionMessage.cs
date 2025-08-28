namespace SensorMessages;

// sensor.cyclist.request.{intersection_id}
public record CyclistDetectionMessage(
    Guid DetectionId,
    Guid IntersectionId,
    int Count,
    DateTime Timestamp
);