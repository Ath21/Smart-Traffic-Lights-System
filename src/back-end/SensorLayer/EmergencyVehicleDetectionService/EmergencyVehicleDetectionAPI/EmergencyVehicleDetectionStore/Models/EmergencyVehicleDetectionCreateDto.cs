using System;

namespace EmergencyVehicleDetectionStore.Models;

public class EmergencyVehicleDetectionCreateDto
{
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Detected { get; set; }
}