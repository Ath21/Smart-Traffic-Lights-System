using System;

namespace TrafficAnalyticsStore.Models.Responses;

public class IncidentResponse
{
    public Guid AlertId { get; set; }
    public Guid IntersectionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
