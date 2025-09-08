namespace SensorMessages;

// sensor.cyclist.request.{intersection_id}
public record CyclistCountMessage(
    Guid DetectionId,
    Guid IntersectionId,
    int Count,
    DateTime Timestamp
);