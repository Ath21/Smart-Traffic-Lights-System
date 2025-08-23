namespace TrafficMessages;

// traffic.light.update.<intersection_id>
public record TrafficLightUpdateMessage(
    Guid IntersectionId,
    Guid LightId,
    string CurrentState,
    DateTime UpdatedAt
);