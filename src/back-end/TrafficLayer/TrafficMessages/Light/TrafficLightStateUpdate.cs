namespace TrafficMessages.Light;

public record TrafficLightStateUpdate(
    string IntersectionId,
    string CurrentPattern,     // e.g., "RED-GREEN-RED"
    DateTime Timestamp
);