using System;

namespace TrafficAnalyticsStore.Models.Dtos;

public class CongestionDto
{
    public Guid IntersectionId { get; set; }
    public string CongestionLevel { get; set; } = string.Empty;
    public int VehicleCount { get; set; }
    public float AvgSpeed { get; set; }
    public DateTime Timestamp { get; set; }
}
