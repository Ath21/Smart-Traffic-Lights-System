using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightData.Entities;

// Update by : Traffic Light Coordinator Service
// Read by   : Traffic Light Coordinator Service
[Table("traffic_configurations")]
public class TrafficConfigurationEntity
{
    // unique configuration ID
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ConfigurationId { get; set; }

    // foreign key reference
    [Required]
    public int IntersectionId { get; set; }

    // navigation property
    [ForeignKey(nameof(IntersectionId))]
    public IntersectionEntity? Intersection { get; set; }

    // operational mode: Auto, Manual, Failover
    [Required, MaxLength(50)]
    public string Mode { get; set; } = "Auto";

    // default duration for green/red cycle
    [Required]
    public int DefaultDurationSec { get; set; } = 60;

    // adaptive timing flag
    public bool AdaptiveControlEnabled { get; set; } = true;

    // active plan: Standard, Peak, Night
    [Required, MaxLength(20)]
    public string TimePlan { get; set; } = "Standard";

    // last configuration update (UTC)
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

