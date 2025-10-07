using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightData.Entities;

// Update by : Traffic Light Coordinator Service
// Read by   : Traffic Light Coordinator Service
[Table("intersections")]
public class IntersectionEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IntersectionId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public int LightCount { get; set; } = 0;

    [Required]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TrafficLightEntity> TrafficLights { get; set; } = new List<TrafficLightEntity>();

    public ICollection<TrafficConfigurationEntity> Configurations { get; set; } = new List<TrafficConfigurationEntity>();
}

