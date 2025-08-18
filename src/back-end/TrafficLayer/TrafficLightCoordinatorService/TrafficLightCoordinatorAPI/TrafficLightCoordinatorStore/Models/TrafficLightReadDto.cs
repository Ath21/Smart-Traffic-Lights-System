using System;

namespace TrafficLightCoordinatorStore.Models;

public class TrafficLightReadDto
{
    public Guid LightId { get; set; }
    public Guid IntersectionId { get; set; }
    public string CurrentState { get; set; } = "Red";
    public DateTime UpdatedAt { get; set; }
}