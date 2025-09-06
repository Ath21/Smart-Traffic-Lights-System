using System;

namespace DetectionData.Collection.Detection;

public class EmergencyVehicleDetection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
    public Guid IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string? Type { get; set; }
    public int? PriorityLevel { get; set; }
}
