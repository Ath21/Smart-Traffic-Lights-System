using System;

namespace TrafficLightCoordinatorStore.Models;

public class IntersectionReadDto
{
    public Guid IntersectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public LatLngDto? Location { get; set; }
    public string? Description { get; set; }
    public DateTime InstalledAt { get; set; }
    public string Status { get; set; } = "inactive";
}