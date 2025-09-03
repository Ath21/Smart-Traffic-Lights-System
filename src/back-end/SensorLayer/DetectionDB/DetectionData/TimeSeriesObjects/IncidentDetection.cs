using System;

namespace DetectionData.TimeSeriesObjects;

public class IncidentDetection
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Description { get; set; } = string.Empty;
}
