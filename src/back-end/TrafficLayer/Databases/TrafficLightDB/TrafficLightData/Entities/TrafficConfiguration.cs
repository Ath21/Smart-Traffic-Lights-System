using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightData.Entities;

[Table("traffic_configurations")]
public class TrafficConfiguration
{
    [Key, Column("config_id")]
    public Guid ConfigId { get; set; }

    [Column("intersection_id")]
    public Guid IntersectionId { get; set; }

    public Intersection? Intersection { get; set; }

    // PostgreSQL jsonb → MSSQL nvarchar(max)
    [Column("pattern", TypeName = "nvarchar(max)")]
    public string Pattern { get; set; } = """{"phases":[],"cycle":0}""";


    [Column("effective_from")]
    public DateTimeOffset EffectiveFrom { get; set; } = DateTimeOffset.UtcNow;

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("change_ref"), MaxLength(128)]
    public string? ChangeRef { get; set; }

    [Column("created_by"), MaxLength(256)]
    public string? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
