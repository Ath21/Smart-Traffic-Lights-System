using System;

namespace TrafficDataAnalyticsStore.Models;

public class CongestionAlertDto
{
    public string IntersectionId { get; set; } = default!;
    public double CongestionLevel { get; set; }         // required for message
    public string Severity { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime Timestamp { get; set; }   
}
