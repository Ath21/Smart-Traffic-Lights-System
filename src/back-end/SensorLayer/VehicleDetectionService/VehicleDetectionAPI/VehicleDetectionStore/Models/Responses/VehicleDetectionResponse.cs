using System;

namespace VehicleDetectionStore.Models.Responses;

public class VehicleDetectionResponse
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public int VehicleCount { get; set; }
    public float AvgSpeed { get; set; }
}
