using System;

namespace EmergencyVehicleDetectionStore.Models;

public class EmergencyVehicleDetectionReadDto
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Detected { get; set; }
}
