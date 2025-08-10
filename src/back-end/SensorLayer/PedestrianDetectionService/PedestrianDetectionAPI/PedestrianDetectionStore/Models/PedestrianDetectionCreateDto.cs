using System;

namespace PedestrianDetectionStore.Models;

public class PedestrianDetectionCreateDto
{
    public Guid IntersectionId { get; set; }
    public int Count { get; set; }
    public DateTime Timestamp { get; set; }
}
