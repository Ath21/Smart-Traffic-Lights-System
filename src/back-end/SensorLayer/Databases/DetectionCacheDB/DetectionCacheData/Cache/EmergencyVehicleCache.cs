using System;

namespace DetectionCacheData.Cache;

public class EmergencyVehicleCache
{
    public Guid IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string? Type { get; set; }        // e.g., "ambulance", "firetruck"
    public int PriorityLevel { get; set; }   // 1 = highest priority
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
