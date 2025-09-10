namespace DetectionStore.Models.Responses;

public class EmergencyVehicleResponse
{
    public Guid IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string? Type { get; set; }
    public int? PriorityLevel { get; set; }
    public DateTime Timestamp { get; set; }
}