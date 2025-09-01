using System;

namespace IntersectionControllerStore.Models.Dtos;

public class TrafficConfigurationDto
{
    public Guid ConfigId { get; set; }
    public Guid IntersectionId { get; set; }
    public string Pattern { get; set; } = "{}";
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
}