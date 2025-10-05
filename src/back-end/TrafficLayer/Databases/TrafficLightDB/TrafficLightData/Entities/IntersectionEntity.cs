using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightData.Entities;

// Update by : Traffic Light Coordinator Service
// Read by   : Traffic Light Coordinator Service
[Table("intersections")]
public class IntersectionEntity
{
    // unique intersection ID
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IntersectionId { get; set; }

    // intersection name (e.g. "Kentriki Pyli")
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // description or GPS location
    [MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    // total traffic lights at this intersection
    [Required]
    public int LightCount { get; set; } = 0;

    // operational flag
    [Required]
    public bool IsActive { get; set; } = true;

    // creation timestamp (UTC)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // traffic lights at this intersection
    public ICollection<TrafficLightEntity> TrafficLights { get; set; } = new List<TrafficLightEntity>();

    // traffic configurations for this intersection
    public ICollection<TrafficConfigurationEntity> Configurations { get; set; } = new List<TrafficConfigurationEntity>();
}

