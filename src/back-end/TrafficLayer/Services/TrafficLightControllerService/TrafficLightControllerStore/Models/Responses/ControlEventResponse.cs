using System;

namespace TrafficLightControllerStore.Models.Responses;

public class ControlEventResponse
{
    public Guid IntersectionId { get; set; }
    public Guid LightId { get; set; }
    public string Command { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
