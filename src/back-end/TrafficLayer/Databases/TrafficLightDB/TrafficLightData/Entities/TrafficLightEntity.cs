using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightData.Entities;

// Update by : Traffic Light Coordinator Service
// Read by   : Traffic Light Coordinator Service
[Table("traffic_lights")]
public class TrafficLightEntity
{
    // unique light ID
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LightId { get; set; }

    // foreign key reference
    [Required]
    public int IntersectionId { get; set; }

    // navigation property
    [ForeignKey(nameof(IntersectionId))]
    public IntersectionEntity? Intersection { get; set; }

    // name (e.g. "Dimitsanas", "Edessis")
    [Required, MaxLength(50)]
    public string LightName { get; set; } = string.Empty;

    // operational state
    [Required]
    public TrafficLightState CurrentState { get; set; } = TrafficLightState.Red; 

    // duration of current state in seconds
    [Required]
    public int DurationSec { get; set; } = 60; 

    // direction label (N, S, E, W)
    [MaxLength(20)]
    public string Direction { get; set; } = string.Empty;

    // indicates lamp is functional
    public bool IsOperational { get; set; } = true; 

    // last state update timestamp (UTC)
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}
