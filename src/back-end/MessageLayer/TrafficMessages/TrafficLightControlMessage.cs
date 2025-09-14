namespace TrafficMessages;

// traffic.light.control.{intersection_id}.{light_id}
public record TrafficLightControlMessage(
    string Intersection,
    string Light,
    string NewState,
    DateTime IssuedAt,
    int? Duration = null,
    string? Reason = null
);

