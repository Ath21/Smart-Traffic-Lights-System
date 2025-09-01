using System;

namespace IntersectionControllerData.Entities;


public class TrafficConfiguration
{
    public Guid ConfigId { get; set; }
    public Guid IntersectionId { get; set; }
    public string Pattern { get; set; } = "{}"; // JSON with phases, durations
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
}
