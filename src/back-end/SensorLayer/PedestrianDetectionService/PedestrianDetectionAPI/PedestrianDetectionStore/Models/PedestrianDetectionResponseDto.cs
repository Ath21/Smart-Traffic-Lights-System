using System;

namespace PedestrianDetectionStore.Models;

public class PedestrianDetectionResponseDto
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public int Count { get; set; }
    public DateTime Timestamp { get; set; }
}
