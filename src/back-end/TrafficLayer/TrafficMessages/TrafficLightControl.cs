namespace TrafficMessages;

public record TrafficLightControl(
    string IntersectionId,     // e.g., "INT-004"
    string ControlPattern,     // e.g., "RED-GREEN-RED", "YELLOW-BLINK", etc.
    int DurationSeconds,       // How long the pattern is active
    string TriggeredBy,        // e.g., "ManualOverride", "AIController"
    DateTime Timestamp         // UTC timestamp
);