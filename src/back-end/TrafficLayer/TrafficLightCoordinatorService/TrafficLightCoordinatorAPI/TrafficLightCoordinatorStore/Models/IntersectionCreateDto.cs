using System;

namespace TrafficLightCoordinatorStore.Models;

public class IntersectionCreateDto
{
    public string Name { get; set; } = string.Empty;
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? Description { get; set; }
    public DateTime? InstalledAt { get; set; }
    public string Status { get; set; } = "active";
}
