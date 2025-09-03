using System;

namespace VehicleDetectionStore.Models.Requests;

public class VehicleDetectionRequest
{
    public int VehicleCount { get; set; }
    public float AvgSpeed { get; set; }
    public DateTime? Timestamp { get; set; }
}
