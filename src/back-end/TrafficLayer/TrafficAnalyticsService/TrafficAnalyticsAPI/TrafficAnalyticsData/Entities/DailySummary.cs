using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficAnalyticsData.Entities;

[Table("daily_summaries")]
public class DailySummary
{
    [Key]
    [Column("summary_id")]
    public Guid SummaryId { get; set; }

    [Column("intersection_id")]
    public Guid IntersectionId { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("avg_speed")]
    public float AvgSpeed { get; set; }

    [Column("vehicle_count")]
    public int VehicleCount { get; set; }

    [Column("congestion_level")]
    [MaxLength(50)]
    public string CongestionLevel { get; set; } = string.Empty;
}
