using System;

namespace DetectionData.TimeSeriesObjects;

public class PublicTransportDetection
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string RouteId { get; set; } = string.Empty;
}
