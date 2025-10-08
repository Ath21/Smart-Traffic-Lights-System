using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficAnalyticsData.Entities;

// Updated by : Traffic Analytics Service
// Read by    : Traffic Analytics Service
[Table("daily_summaries")]
public class DailySummaryEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SummaryId { get; set; }

    [Required]
    public int IntersectionId { get; set; }

    [Required]
    public string? Intersection { get; set; } 

    [Required]
    public DateTime Date { get; set; } 

    public int TotalVehicles { get; set; }
    public int TotalPedestrians { get; set; }
    public int TotalCyclists { get; set; }
    public double AverageSpeedKmh { get; set; }
    public double AverageWaitTimeSec { get; set; }
}

/*

{
  "summaryId": 184,
  "intersectionId": 2,
  "intersection": "Agiou Spyridonos",
  "date": "2025-10-07T00:00:00Z",
  "totalVehicles": 1458,
  "totalPedestrians": 242,
  "totalCyclists": 32,
  "averageSpeedKmh": 38.7,
  "averageWaitTimeSec": 12.4
}

*/