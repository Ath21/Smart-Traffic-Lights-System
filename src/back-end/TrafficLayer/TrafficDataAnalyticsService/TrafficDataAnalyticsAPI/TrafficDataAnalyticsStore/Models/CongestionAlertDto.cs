using System;

namespace TrafficDataAnalyticsStore.Models;

public class CongestionAlertDto
{
    public string IntersectionId { get; set; } = "";
    public string Severity { get; set; } = "LOW";
}
