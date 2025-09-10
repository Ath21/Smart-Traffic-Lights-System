namespace DetectionStore.Models.Dtos;

public class EmergencyVehicleDto
{
    public Guid IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string? Type { get; set; }
    public int? PriorityLevel { get; set; }
    public DateTime Timestamp { get; set; }
}