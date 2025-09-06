using System;

namespace DetectionCacheData.Cache;

public class PedestrianCountCache
{
    public Guid IntersectionId { get; set; }
    public int PedestrianCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
