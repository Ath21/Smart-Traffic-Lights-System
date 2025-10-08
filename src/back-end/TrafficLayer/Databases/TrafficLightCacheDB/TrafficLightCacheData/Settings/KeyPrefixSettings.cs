using System;

namespace TrafficLightCacheData.Settings;

public class KeyPrefixSettings
{
    // =========================
    // Core State Keys
    // =========================
    public string? State { get; set; }                     // traffic_light:{intersection}:{light}:state
    public string? Duration { get; set; }                  // traffic_light:{intersection}:{light}:duration
    public string? LastUpdate { get; set; }                // traffic_light:{intersection}:{light}:last_update

    // =========================
    // Synchronization Keys
    // =========================
    public string? CycleDuration { get; set; }             // traffic_light:{intersection}:{light}:cycle_duration
    public string? Offset { get; set; }                    // traffic_light:{intersection}:{light}:offset
    public string? CycleProgress { get; set; }             // traffic_light:{intersection}:{light}:cycle_progress

    // =========================
    // Configuration & Priority
    // =========================
    public string? Mode { get; set; }                      // traffic_light:{intersection}:{light}:mode
    public string? Priority { get; set; }                  // traffic_light:{intersection}:{light}:priority

    // =========================
    // Failover Keys
    // =========================
    public string? CachedPhases { get; set; }              // traffic_light:{intersection}:{light}:cached_phases
    public string? FailoverActive { get; set; }            // traffic_light:{intersection}:{light}:failover

    // =========================
    // Diagnostic Keys
    // =========================
    public string? Heartbeat { get; set; }                 // traffic_light:{intersection}:{light}:heartbeat
    public string? LastCoordinatorSync { get; set; }       // traffic_light:{intersection}:last_sync
}
