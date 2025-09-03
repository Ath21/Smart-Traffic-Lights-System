using System;

namespace TrafficLightControlStore.Models.Dtos;

public class ControlEventDto
{
    public Guid IntersectionId { get; set; }
    public Guid LightId { get; set; }
    public string Command { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}