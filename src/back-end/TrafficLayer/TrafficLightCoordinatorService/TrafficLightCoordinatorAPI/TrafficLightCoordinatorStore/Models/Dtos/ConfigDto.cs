using System;

namespace TrafficLightCoordinatorStore.Models.Dtos;

public class ConfigDto
{
    public Guid ConfigId { get; set; }
    public Guid IntersectionId { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public DateTimeOffset EffectiveFrom { get; set; }
}
