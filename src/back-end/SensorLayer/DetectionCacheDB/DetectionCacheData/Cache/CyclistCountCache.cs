using System;

namespace DetectionCacheData.Cache;

public class CyclistCountCache
{
    public Guid IntersectionId { get; set; }
    public int CyclistCount { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
