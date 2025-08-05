using System;

namespace DetectionData.TimeSeriesObjects;

public class PedestrianDetection
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public int Count { get; set; }
}