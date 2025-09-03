using System;

namespace PedestrianDetectionStore.Models.Responses;


public class PedestrianDetectionResponse
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public int Count { get; set; }
}
