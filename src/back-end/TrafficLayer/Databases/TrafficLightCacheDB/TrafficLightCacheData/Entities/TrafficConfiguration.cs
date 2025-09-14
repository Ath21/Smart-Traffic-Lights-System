namespace TrafficLightCacheData.Entities;

public class TrafficConfiguration
{
    public string Intersection { get; set; } = string.Empty;  // Config belongs to an intersection
    public string Name { get; set; } = string.Empty;          // e.g., "DefaultCycle", "RushHourPlan"

    // JSON with phases, durations, and rules
    // Example:
    // {
    //   "phases": [
    //     { "light": "west", "state": "Green", "duration": 30 },
    //     { "light": "west", "state": "Yellow", "duration": 5 }
    //   ]
    // }
    public string Pattern { get; set; } = "{}";               

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }                 // Optional end time (for temporary configs)
}
