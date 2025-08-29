using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightCoordinatorData.Entities;

[Table("traffic_lights")]
public class TrafficLight
{
    [Key, Column("light_id")]
    public Guid LightId { get; set; }

    [Column("intersection_id")]
    public Guid IntersectionId { get; set; }

    public Intersection? Intersection { get; set; }

    [Column("current_state")]
    public TrafficLightState CurrentState { get; set; } = TrafficLightState.RED;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
