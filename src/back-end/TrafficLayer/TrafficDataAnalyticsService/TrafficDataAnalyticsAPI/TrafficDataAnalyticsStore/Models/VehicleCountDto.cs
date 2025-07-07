using System;

namespace TrafficDataAnalyticsStore.Models;

public class VehicleCountDto
{
    public string IntersectionId { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public int Count { get; set; }
    public string LaneId { get; set; } = "";
}
