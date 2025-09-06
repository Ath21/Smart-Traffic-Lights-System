using System;

namespace DetectionData.Collection.Detection;

public class IncidentDetection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
    public Guid IntersectionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Severity { get; set; }
    public string Description { get; set; } = string.Empty;
}