namespace SensorMessages.Data;


public record IncidentDetectionMessage(
    Guid IntersectionId,
    string IncidentType, // e.g., collision, obstruction
    string Severity,     // e.g., low, medium, high
    string Description,
    DateTime Timestamp
);