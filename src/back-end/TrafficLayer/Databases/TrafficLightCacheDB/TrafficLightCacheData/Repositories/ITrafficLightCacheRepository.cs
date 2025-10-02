using System;

namespace TrafficLightCacheData.Repositories;

public interface ITrafficLightCacheRepository
{
    Task SetStateAsync(int intersection, string state);
    Task<string?> GetStateAsync(int intersection);

    Task SetDurationAsync(int intersection, int seconds);
    Task<int?> GetDurationAsync(int intersection);

    Task SetLastUpdateAsync(int intersection, DateTime timestamp);
    Task<DateTime?> GetLastUpdateAsync(int intersection);

    Task SetPriorityAsync(int intersection, string priority);
    Task<string?> GetPriorityAsync(int intersection);

    Task SetQueueLengthAsync(int intersection, int length);
    Task<int?> GetQueueLengthAsync(int intersection);
}
