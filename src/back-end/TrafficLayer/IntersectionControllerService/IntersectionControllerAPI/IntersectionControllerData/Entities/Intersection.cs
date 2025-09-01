using System;

namespace IntersectionControllerData.Entities;

public class Intersection
{
    public Guid IntersectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty; // store as GeoJSON string
    public string? Description { get; set; }
    public DateTime InstalledAt { get; set; }
    public string Status { get; set; } = "Active";
}
