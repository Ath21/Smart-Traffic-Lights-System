using System;

namespace TrafficAnalyticsStore.Models.Responses;

public class SummaryResponse
{
    public Guid SummaryId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Date { get; set; }
    public float AvgSpeed { get; set; }
    public int VehicleCount { get; set; }
    public string CongestionLevel { get; set; } = string.Empty;
}