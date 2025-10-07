using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightData.Entities;

// Update by : Traffic Light Coordinator Service
// Read by   : Traffic Light Coordinator Service
[Table("traffic_configurations")]
public class TrafficConfigurationEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ConfigurationId { get; set; }

    [Required]
    public int IntersectionId { get; set; }

    [ForeignKey(nameof(IntersectionId))]
    public IntersectionEntity? Intersection { get; set; }

    [Required, MaxLength(50)]
    public string Mode { get; set; }

    [Required]
    public int DefaultDurationSec { get; set; }

    public bool AdaptiveControlEnabled { get; set; } 

    [Required, MaxLength(20)]
    public string TimePlan { get; set; } 

    public DateTime LastUpdated { get; set; }
}

