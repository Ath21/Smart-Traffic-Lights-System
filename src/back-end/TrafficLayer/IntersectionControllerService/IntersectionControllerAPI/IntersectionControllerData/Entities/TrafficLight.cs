using System;

namespace IntersectionControllerData.Entities;

public class TrafficLight
{
    public Guid LightId { get; set; }
    public Guid IntersectionId { get; set; }
    public string CurrentState { get; set; } = "Red"; // Red, Green, Yellow
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}