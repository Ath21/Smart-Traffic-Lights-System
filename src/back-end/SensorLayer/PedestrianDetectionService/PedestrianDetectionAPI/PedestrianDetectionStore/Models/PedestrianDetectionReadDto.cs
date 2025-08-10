using System;

namespace PedestrianDetectionStore.Models;

public class PedestrianDetectionReadDto
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public int Count { get; set; }
    public DateTime Timestamp { get; set; }
}
