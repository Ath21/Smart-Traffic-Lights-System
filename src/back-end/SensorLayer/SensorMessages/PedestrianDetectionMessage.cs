namespace SensorMessages;

// sensor.pedestrian.request.{intersection_id}
public record PedestrianDetectionMessage(
    Guid DetectionId,
    Guid IntersectionId,
    int Count,
    DateTime Timestamp
);