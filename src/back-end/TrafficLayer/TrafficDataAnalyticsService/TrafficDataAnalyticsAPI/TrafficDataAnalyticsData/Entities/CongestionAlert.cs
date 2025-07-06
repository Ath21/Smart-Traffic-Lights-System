using System;

namespace TrafficDataAnalyticsData.Entities;

public class CongestionAlert
{
    public Guid AlertId { get; set; }
    public string IntersectionId { get; set; } = "";
    public string Severity { get; set; } = "Low"; // Default severity level

    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
}
