using System;

namespace TrafficLightCacheData.Repositories;

public interface ITrafficLightCacheRepository
{
    // Core state
    Task SetStateAsync(int intersectionId, int lightId, string state);
    Task<string?> GetStateAsync(int intersectionId, int lightId);

    Task SetDurationAsync(int intersectionId, int lightId, int seconds);
    Task<int> GetDurationAsync(int intersectionId, int lightId);

    Task SetLastUpdateAsync(int intersectionId, int lightId, DateTime timestamp);
    Task<DateTime?> GetLastUpdateAsync(int intersectionId, int lightId);

    // Synchronization
    Task SetCycleDurationAsync(int intersectionId, int lightId, int seconds);
    Task<int> GetCycleDurationAsync(int intersectionId, int lightId);

    Task SetOffsetAsync(int intersectionId, int lightId, int seconds);
    Task<int> GetOffsetAsync(int intersectionId, int lightId);

    // Configuration & Priority
    Task SetModeAsync(int intersectionId, int lightId, string mode);
    Task<string?> GetModeAsync(int intersectionId, int lightId);

    Task SetPriorityAsync(int intersectionId, int lightId, int priority);
    Task<int> GetPriorityAsync(int intersectionId, int lightId);

    // Failover
    Task SetFailoverAsync(int intersectionId, int lightId, bool active);
    Task<bool> GetFailoverAsync(int intersectionId, int lightId);

    // Diagnostics
    Task SetHeartbeatAsync(int intersectionId, int lightId);
    Task SetCoordinatorSyncAsync(int intersectionId);
}
