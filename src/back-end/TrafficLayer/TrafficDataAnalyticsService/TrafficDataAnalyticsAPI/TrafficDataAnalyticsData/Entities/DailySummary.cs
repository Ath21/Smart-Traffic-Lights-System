using System;

namespace TrafficDataAnalyticsData.Entities;

public class DailySummary
{
    public Guid SummaryId { get; set; }
    public string IntersectionId { get; set; } = "";
    public DateTime Date { get; set; }

    public float AverageWaitTime { get; set; }
    public String PeakHours { get; set; } = "{}"; // JSON string for peak hours
    public int TotalVehicleCount { get; set; }
}
