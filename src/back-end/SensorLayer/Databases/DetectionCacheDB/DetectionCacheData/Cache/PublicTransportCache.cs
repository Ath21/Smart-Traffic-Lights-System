using System;

namespace DetectionCacheData.Cache;

public class PublicTransportCache
{
    public Guid IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string? Mode { get; set; }        // e.g., "bus", "tram"
    public string? RouteId { get; set; }     // bus line or route identifier
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
