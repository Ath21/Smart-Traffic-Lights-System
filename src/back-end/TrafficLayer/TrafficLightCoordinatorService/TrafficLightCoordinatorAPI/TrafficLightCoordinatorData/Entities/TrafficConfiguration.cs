using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightCoordinatorData.Entities;

[Table("traffic_configurations")]
public class TrafficConfiguration
{
    [Key, Column("config_id")]
    public Guid ConfigId { get; set; }

    [Column("intersection_id")]
    public Guid IntersectionId { get; set; }

    public Intersection? Intersection { get; set; }

    // JSON schedule pattern { phases: [...], updated_at: ... }
    [Column("pattern", TypeName = "jsonb")]
    public string Pattern { get; set; } = """{ "phases": [], "updated_at": null }""";

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
