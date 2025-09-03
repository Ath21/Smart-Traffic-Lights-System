using System;

namespace CyclistDetectionStore.Models.Responses;

public class CyclistDetectionResponse
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public int Count { get; set; }
}