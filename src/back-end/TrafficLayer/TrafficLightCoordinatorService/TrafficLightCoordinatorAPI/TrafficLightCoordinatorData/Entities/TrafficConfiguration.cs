// src/Coordinator.Infrastructure/Entities/TrafficConfigurationEntity.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TrafficLightCoordinatorData.Entities;

public class TrafficConfiguration
{
    [Key] public Guid ConfigId { get; set; }

    [ForeignKey(nameof(Intersection))]
    public Guid IntersectionId { get; set; }

    // JSON (jsonb) storing the plan/pattern (phases, timings, groups)
    public JsonDocument Pattern { get; set; } = JsonDocument.Parse("{}");

    public DateTime EffectiveFrom { get; set; }

    // Navigation
    public Intersection? Intersection { get; set; }
}
