using System;

namespace EmergencyVehicleDetectionStore.Models.Responses;

public class EmergencyVehicleDetectionResponse
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Detected { get; set; }
}
