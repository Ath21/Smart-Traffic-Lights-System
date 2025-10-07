using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightData.Entities;

// Update by : Traffic Light Coordinator Service
// Read by   : Traffic Light Coordinator Service
[Table("traffic_lights")]
public class TrafficLightEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LightId { get; set; }

    [Required]
    public int IntersectionId { get; set; }

    [ForeignKey(nameof(IntersectionId))]
    public IntersectionEntity? Intersection { get; set; }

    [Required, MaxLength(50)]
    public string LightName { get; set; } = string.Empty;

    [Required]
    public TrafficLightState CurrentState { get; set; } = TrafficLightState.Red; 

    [Required]
    public int DurationSec { get; set; } = 60; 

    [MaxLength(20)]
    public string Direction { get; set; } = string.Empty;

    public bool IsOperational { get; set; } = true; 

    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}
