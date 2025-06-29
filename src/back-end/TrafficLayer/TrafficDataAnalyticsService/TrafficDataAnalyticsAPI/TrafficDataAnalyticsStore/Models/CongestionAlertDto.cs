using System;

namespace TrafficDataAnalyticsStore.Models;

public class CongestionAlertDto
{
    public string IntersectionId { get; set; } = default!;
    public string Severity { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime TimeStamp { get; set; }
}
