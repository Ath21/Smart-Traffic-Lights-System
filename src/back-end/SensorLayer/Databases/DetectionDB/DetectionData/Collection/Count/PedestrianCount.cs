using System;

namespace DetectionData.Collection.Count;

public class PedestrianCount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
    public Guid IntersectionId { get; set; }
    public int Count { get; set; }
}