using System;

namespace DetectionData.Collection.Count;

public class VehicleCount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
    public Guid IntersectionId { get; set; }
    public string LaneId { get; set; } = string.Empty;
    public int Count { get; set; }
}
