using System;

namespace DetectionCacheData.Cache;

public class VehicleCountCache
{
    public Guid IntersectionId { get; set; }
    public int VehicleCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
