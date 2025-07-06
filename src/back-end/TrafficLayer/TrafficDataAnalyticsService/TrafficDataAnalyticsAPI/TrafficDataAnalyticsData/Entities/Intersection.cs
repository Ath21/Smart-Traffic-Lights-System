using System;

namespace TrafficDataAnalyticsData.Entities;

public class Intersection
{
    public string IntersectionId { get; set; } = "";

    public string LocationName { get; set; } = "";

    public string Coordinates { get; set; } = "{}"; // JSON string for coordinates

    public string Status { get; set; } = "active"; // Default status is active
}
