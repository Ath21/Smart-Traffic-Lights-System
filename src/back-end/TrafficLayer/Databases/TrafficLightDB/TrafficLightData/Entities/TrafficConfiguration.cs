using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrafficLightData.Entities;

public class TrafficConfiguration
{
    public int ConfigId { get; set; }
    public int IntersectionId { get; set; }
    public string Pattern { get; set; } = "{}"; // JSON
    public Intersection? Intersection { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
