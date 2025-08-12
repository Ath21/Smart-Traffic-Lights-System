using System;

namespace TrafficLightControlStore.Models;

public class TrafficLightStateResponse
{
    public Guid LightId { get; set; }
    public string CurrentState { get; set; }
    public DateTime UpdatedAt { get; set; }
}
