// src/Coordinator.Infrastructure/Entities/TrafficConfigurationEntity.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TrafficLightCoordinatorData.Entities;

public class TrafficConfiguration
{
    public Guid Id { get; set; }                        // config_id (PK)
    public Guid IntersectionId { get; set; }            // (FK)
    public JsonDocument Pattern { get; set; } = JsonDocument.Parse("{}"); // jsonb
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    // Nav
    public Intersection? Intersection { get; set; }
}
