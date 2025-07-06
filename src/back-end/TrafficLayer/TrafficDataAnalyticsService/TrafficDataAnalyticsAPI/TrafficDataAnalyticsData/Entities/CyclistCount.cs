using System;

namespace TrafficDataAnalyticsData.Entities;

public class CyclistCount
{
    public Guid Id { get; set; }

    public string IntersectionId { get; set; } = "";

    public DateTime Timestamp { get; set; }

    public int Count { get; set; }
}
