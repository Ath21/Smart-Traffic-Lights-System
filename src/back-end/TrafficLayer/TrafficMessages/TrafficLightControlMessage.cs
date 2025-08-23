namespace TrafficMessages;


// traffic.light.control.<intersection_id>.<light_id>
public record TrafficLightControlMessage(
    Guid IntersectionId,
    Guid LightId,
    string NewState,   // GREEN, RED, YELLOW
    DateTime IssuedAt
);
