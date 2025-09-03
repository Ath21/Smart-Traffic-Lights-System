using System;

namespace TrafficLightControllerStore.Models.Dtos;

public class TrafficLightDto
{
    public Guid IntersectionId { get; set; }
    public Guid LightId { get; set; }
    public string State { get; set; } = string.Empty;

    // Extra metadata for monitoring
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}