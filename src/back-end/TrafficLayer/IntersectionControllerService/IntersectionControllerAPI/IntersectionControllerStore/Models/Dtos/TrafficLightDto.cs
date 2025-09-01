using System;

namespace IntersectionControllerStore.Models.Dtos;

public class TrafficLightDto
{
    public Guid LightId { get; set; }
    public Guid IntersectionId { get; set; }
    public string CurrentState { get; set; } = "Red";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}