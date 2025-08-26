using System;

namespace TrafficDataAnalyticsStore.Models.Responses;

public class CongestionResponse
{
    public Guid IntersectionId { get; set; }
    public string CongestionLevel { get; set; } = string.Empty;
    public int VehicleCount { get; set; }
    public float AvgSpeed { get; set; }
    public DateTime Timestamp { get; set; }
}