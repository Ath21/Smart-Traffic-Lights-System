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
    public string Mode { get; set; } = "Standard";

    [Required, MaxLength(20)]
    public string TimePlan { get; set; } = "Day";

    public int CycleDurationSec { get; set; } = 60;
    public int GlobalOffsetSec { get; set; } = 0;

    // Serialized JSON field for phase durations
    public string PhaseDurationsJson { get; set; } = "{}";

    // Serialized JSON field for per-light offsets
    public string LightOffsetsJson { get; set; } = "{}";

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}


/*

{
  "ConfigurationId": 12,
  "IntersectionId": 2,
  "Mode": "Standard",
  "TimePlan": "MorningRush",
  "CycleDurationSec": 60,
  "GlobalOffsetSec": 10,
  "PhaseDurationsJson": "{\"Green\":40, \"Yellow\":5, \"Red\":15}",
  "LightOffsetsJson": "{\"101\":0, \"102\":5, \"103\":10}",
  "LastUpdated": "2025-10-08T07:00:00Z"
}

{
  "ConfigurationId": 13,
  "IntersectionId": 5,
  "Mode": "Peak",
  "TimePlan": "AfternoonPeak",
  "CycleDurationSec": 75,
  "GlobalOffsetSec": 20,
  "PhaseDurationsJson": "{\"Green\":50, \"Yellow\":5, \"Red\":20}",
  "LightOffsetsJson": "{\"501\":0, \"502\":10}",
  "LastUpdated": "2025-10-08T17:00:00Z"
}

{
  "ConfigurationId": 14,
  "IntersectionId": 4,
  "Mode": "Night",
  "TimePlan": "Night",
  "CycleDurationSec": 50,
  "GlobalOffsetSec": 0,
  "PhaseDurationsJson": "{\"Green\":15, \"Yellow\":5, \"Red\":30}",
  "LightOffsetsJson": "{\"201\":0, \"202\":0}",
  "LastUpdated": "2025-10-08T23:00:00Z"
}

*/