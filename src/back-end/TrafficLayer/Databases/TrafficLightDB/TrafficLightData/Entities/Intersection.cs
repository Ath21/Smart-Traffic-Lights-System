using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightData.Entities;

[Table("intersections")]
public class Intersection
{
    [Key, Column("intersection_id")]
    public Guid IntersectionId { get; set; }

    [Column("name"), MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("location", TypeName = "nvarchar(max)")]
    public string? Location { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("installed_at")]
    public DateTimeOffset? InstalledAt { get; set; }

    [Column("status"), MaxLength(50)]
    public string Status { get; set; } = "Active";

    public ICollection<TrafficLight> Lights { get; set; } = new List<TrafficLight>();
    public ICollection<TrafficConfiguration> Configurations { get; set; } = new List<TrafficConfiguration>();
}
