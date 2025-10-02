using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrafficLightData.Entities;

namespace TrafficAnalyticsData.Entities;

public class Alert
{
    public int AlertId { get; set; }            // PK
    public int IntersectionId { get; set; }     // FK
    public string Type { get; set; } = string.Empty;  // e.g. "EMERGENCY", "FAILOVER", "CONFIG_CHANGE"
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Intersection? Intersection { get; set; }
}