using System;

namespace TrafficLightControllerStore.Models.Responses;

public class TrafficLightStatusResponse
{
    public Guid IntersectionId { get; set; }
    public Guid LightId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
}