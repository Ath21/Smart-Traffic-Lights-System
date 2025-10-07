using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrafficLightData.Entities;

namespace TrafficAnalyticsData.Entities;

// Updated by : Traffic Analytics Service
// Read by    : Traffic Analytics Service
[Table("alerts")]
public class AlertEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AlertId { get; set; }

    [Required]
    public int IntersectionId { get; set; }

    [Required, MaxLength(100)]
    public string Intersection { get; set; }

    [Required, MaxLength(50)]
    public string Type { get; set; } // Incident, Congestion

    [MaxLength(500)]
    public string Message { get; set; }

    public DateTime CreatedAt { get; set; }
}

/*

{
  "alertId": 21,
  "intersectionId": 3,
  "intersection": "Dytiki Pyli",
  "type": "Incident",
  "message": "Minor collision detected at southbound lane.",
  "createdAt": "2025-10-07T14:15:05Z"
}

*/