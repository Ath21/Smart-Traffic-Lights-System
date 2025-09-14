namespace TrafficLightCacheData.Entities;

public class TrafficLight
{
    public string Intersection { get; set; } = string.Empty;   // e.g., "ekklhsia"
    public string Light { get; set; } = string.Empty;          // e.g., "west", "north"
    
    public string CurrentState { get; set; } = "Red";          // Red, Green, Yellow
    public int? Duration { get; set; }                         // Remaining seconds for this state
    public string? OverrideState { get; set; }                 // Manual override if active
    public DateTime? OverrideExpiresAt { get; set; }           // When override ends
    public string? OverrideReason { get; set; }                // Why override was set
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Last state update
}
