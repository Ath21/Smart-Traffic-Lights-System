using System;
using DetectionCacheData.Entities;

namespace DetectionCacheData.Repositories.Cache;

public interface ISensorCacheRepository
{
    Task<DetectionCache?> GetSnapshotAsync(Guid intersectionId);
    Task SetSnapshotAsync(Guid intersectionId, DetectionCache snapshot);
}