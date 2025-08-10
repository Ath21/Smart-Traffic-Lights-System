using System;

namespace CyclistDetectionStore.Models;

public class CyclistDetectionCreateDto
{
    public Guid IntersectionId { get; set; }
    public int Count { get; set; }
    public DateTime Timestamp { get; set; }
}