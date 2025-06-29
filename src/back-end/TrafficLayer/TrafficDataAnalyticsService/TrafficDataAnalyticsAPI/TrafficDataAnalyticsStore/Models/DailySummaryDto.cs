using System;

namespace TrafficDataAnalyticsStore.Models;

public class DailySummaryDto
{
    public string IntersectionId { get; set; } = default!;
    public DateTime Date { get; set; }
    public float AvgWaitTime { get; set; }
    public Dictionary<string, int> PeakHours { get; set; } = new();
    public int TotalVehicleCount { get; set; }
}
