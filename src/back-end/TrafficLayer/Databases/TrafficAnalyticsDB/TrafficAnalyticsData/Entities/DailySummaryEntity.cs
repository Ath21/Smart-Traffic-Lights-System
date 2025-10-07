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
    public int SummaryId { get; set; } = 0;

    [Required]
    public int IntersectionId { get; set; } = 0;

    [Required]
    public string Intersection { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int TotalVehicles { get; set; } = 0;
    public int TotalPedestrians { get; set; } = 0;
    public int TotalCyclists { get; set; } = 0;
    public double AverageSpeedKmh { get; set; } = 0.0;
    public double AverageWaitTimeSec { get; set; } = 0.0;
}
