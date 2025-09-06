using System;

namespace DetectionCacheData.Cache;

public class IncidentCache
{
    public Guid IntersectionId { get; set; }
    public string Type { get; set; }         // e.g., "collision", "breakdown"
    public int Severity { get; set; }        // 1 = minor, 5 = severe
    public string? Description { get; set; } // details
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}