using System;

namespace EmergencyVehicleDetectionStore.Models;

public class DetectionResponseDto
{
    public Guid DetectionId { get; set; }
    public string Status { get; set; }
}
