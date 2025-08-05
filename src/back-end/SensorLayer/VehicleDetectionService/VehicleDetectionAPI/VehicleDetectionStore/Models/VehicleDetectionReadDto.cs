using System;

namespace VehicleDetectionStore.Models;

public class VehicleDetectionReadDto
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public int VehicleCount { get; set; }
    public float AvgSpeed { get; set; }
}