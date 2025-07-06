using System;

namespace TrafficDataAnalyticsStore.Models;

public class DailySummaryDto
{
    public string IntersectionId { get; set; } = "";
    public DateTime Date { get; set; }
    public float AvgWaitTime { get; set; }
    public string PeakHours { get; set; } = "{}";
    public int TotalVehicleCount { get; set; }
}
