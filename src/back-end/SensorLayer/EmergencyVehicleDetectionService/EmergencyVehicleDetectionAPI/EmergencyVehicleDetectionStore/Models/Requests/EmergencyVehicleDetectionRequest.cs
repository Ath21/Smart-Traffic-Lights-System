using System;

namespace EmergencyVehicleDetectionStore.Models.Requests;

public class EmergencyVehicleDetectionRequest
{
    public bool Detected { get; set; }
    public DateTime? Timestamp { get; set; }
}
