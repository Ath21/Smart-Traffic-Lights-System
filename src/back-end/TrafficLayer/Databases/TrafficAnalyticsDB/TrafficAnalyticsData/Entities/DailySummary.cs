using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficAnalyticsData.Entities;

public class DailySummary
{
    public int SummaryId { get; set; }                // PK
    public int IntersectionId { get; set; }           // FK → TrafficLightDB.IntersectionId
    public DateTime Date { get; set; }
    public float AvgSpeed { get; set; }
    public int VehicleCount { get; set; }
    public string CongestionLevel { get; set; } = string.Empty;
}
