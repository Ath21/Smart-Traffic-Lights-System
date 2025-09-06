using System;

namespace TrafficLightCoordinatorStore.Models.Responses;

public class ConfigResponse
{
    public Guid ConfigId { get; set; }
    public Guid IntersectionId { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public DateTimeOffset EffectiveFrom { get; set; }
}