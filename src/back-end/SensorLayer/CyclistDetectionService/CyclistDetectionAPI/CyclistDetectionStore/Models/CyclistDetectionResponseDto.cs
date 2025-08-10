using System;

namespace CyclistDetectionStore.Models;

public class CyclistDetectionResponseDto
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public int Count { get; set; }
    public DateTime Timestamp { get; set; }
}