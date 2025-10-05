using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficAnalyticsData.Entities;

// Updated by : Traffic Analytics Service
// Read by    : Traffic Analytics Service
[Table("daily_summaries")]
public class DailySummaryEntity
{
    // unique summary record ID
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SummaryId { get; set; }

    // reference to intersection
    [Required]
    public int IntersectionId { get; set; }

     // intersection name (e.g. "Agiou Spyridonos")
    [Required]
    public string Intersection { get; set; } = string.Empty;

    // date of summary (UTC)
    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    // total vehicle count per day
    public int TotalVehicles { get; set; }

    // total pedestrian count per day
    public int TotalPedestrians { get; set; }
   
    // total cyclist count per day 
    public int TotalCyclists { get; set; }
 
    // mean speed over the day
    public double AverageSpeedKmh { get; set; }
 
    // mean waiting time over the day
    public double AverageWaitTimeSec { get; set; }
}
