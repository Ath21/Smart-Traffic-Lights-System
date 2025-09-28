using System;

namespace DetectionStore.Models.Dtos;

public class EmergencyVehicleDto
{
    public int IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string Type { get; set; } = default!;
    public int PriorityLevel { get; set; }
    public DateTime Timestamp { get; set; }
}