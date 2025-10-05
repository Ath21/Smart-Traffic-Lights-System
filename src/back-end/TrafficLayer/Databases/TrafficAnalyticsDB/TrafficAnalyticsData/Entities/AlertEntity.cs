using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrafficLightData.Entities;

namespace TrafficAnalyticsData.Entities;

// Updated by : Traffic Analytics Service
// Read by    : Traffic Analytics Service
[Table("alerts")]
public class AlertEntity
{
    // unique alert ID
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AlertId { get; set; }

    // reference to intersection
    [Required]
    public int IntersectionId { get; set; }

    // intersection name (e.g. "Agiou Spyridonos")
    [Required, MaxLength(100)]
    public string Intersection { get; set; } = string.Empty;

    // alert type (e.g. "congestion", "failover")
    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    // alert message (e.g. "Traffic congestion detected")
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    // creation timestamp (UTC)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}