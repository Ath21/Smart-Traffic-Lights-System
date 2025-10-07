using System;

namespace TrafficLightCacheData.Keys;

public static class TrafficLightCacheKeys
{
    // ===============================================================
    // CORE STATE KEYS
    // ===============================================================

    // Current state of a specific traffic light (Red, Green, Yellow, BlinkingYellow, Off)
    // Updated by : Traffic Light Controller (Edge)
    // Read by    : Traffic Light Controller, Intersection Controller
    public static string State(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:state";

    // Duration (in seconds) of the current phase for this light.
    // Updated by : Intersection Controller (Fog)
    // Read by    : Traffic Light Controller, Intersection Controller
    public static string Duration(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:duration";

    // Timestamp of the last state change (UTC serialized as string)
    // Updated by : Traffic Light Controller
    // Read by    : Traffic Light Controller, Intersection Controller, Log Service (optional)
    public static string LastUpdate(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:last_update";

    // ===============================================================
    // SYNCHRONIZATION KEYS (NEW)
    // ===============================================================

    // Total cycle duration for this light (used for synchronization)
    // Value type : int (seconds)
    // Updated by : Intersection Controller
    // Read by    : Traffic Light Controller
    public static string CycleDuration(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:cycle_duration";

    // Phase offset for this light (in seconds, relative to intersection cycle)
    // Value type : int (seconds)
    // Updated by : Intersection Controller
    // Read by    : Traffic Light Controller
    public static string Offset(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:offset";

    // Current cycle progress (useful for analytics or drift detection)
    // Value type : double (seconds elapsed since cycle start)
    // Updated by : Traffic Light Controller (Edge)
    // Read by    : Intersection Controller, Traffic Analytics Service
    public static string CycleProgress(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:cycle_progress";

    // ===============================================================
    // CONFIGURATION & PRIORITY KEYS
    // ===============================================================

    // Active operational mode (Standard, Peak, Night, Manual, Failover)
    // Value type : string
    // Updated by : Intersection Controller
    // Read by    : Traffic Light Controller, Coordinator
    public static string Mode(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:mode";

    // Priority index for emergency preemption or congestion handling.
    // Value type : int (1=Low, 3=High)
    // Updated by : Intersection Controller
    // Read by    : Traffic Light Controller, Intersection Controller
    public static string Priority(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:priority";

    // ===============================================================
    // FAILOVER KEYS
    // ===============================================================

    // Cached previous phase durations (serialized JSON)
    // Value type : string (e.g. {"Green":30,"Yellow":5,"Red":25})
    // Updated by : Intersection Controller
    // Read by    : Traffic Light Controller, Failover Service
    public static string CachedPhases(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:cached_phases";

    // Failover active flag (true/false)
    // Updated by : Failover Service
    // Read by    : Traffic Light Controller, Intersection Controller
    public static string FailoverActive(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:failover";

    // ===============================================================
    // DIAGNOSTIC KEYS
    // ===============================================================

    // Timestamp when last successfully synced with Coordinator
    // Value type : DateTime (string)
    // Updated by : Intersection Controller
    // Read by    : Traffic Light Coordinator, Monitoring
    public static string LastCoordinatorSync(int intersectionId)
        => $"traffic_light:{intersectionId}:last_sync";

    // Heartbeat check for edge controllers
    // Value type : DateTime (string)
    // Updated by : Traffic Light Controller
    // Read by    : Intersection Controller, Monitoring
    public static string Heartbeat(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:heartbeat";
}
