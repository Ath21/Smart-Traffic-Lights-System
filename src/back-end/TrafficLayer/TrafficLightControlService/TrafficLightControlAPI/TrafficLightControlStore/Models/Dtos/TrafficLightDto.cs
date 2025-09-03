using System;

namespace TrafficLightControlStore.Models.Dtos;

public class TrafficLightDto
{
    public Guid IntersectionId { get; set; }
    public Guid LightId { get; set; }
    public string State { get; set; } = string.Empty;
}