namespace TrafficLightCacheData.Entities;

public class Intersection
{
    public string Name { get; set; } = string.Empty;          // e.g., "ekklhsia"
    public string Location { get; set; } = string.Empty;      // GeoJSON (coordinates, polygon)
    public string? Description { get; set; }                  // Free text
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active";            // Active / Inactive / Maintenance
    
    public List<TrafficLight>? Lights { get; set; }           // Lights belonging to this intersection
}
