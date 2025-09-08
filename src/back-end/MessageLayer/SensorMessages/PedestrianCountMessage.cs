namespace SensorMessages;

// sensor.pedestrian.request.{intersection_id}
public record PedestrianCountMessage(
    Guid DetectionId,
    Guid IntersectionId,
    int Count,
    DateTime Timestamp
);