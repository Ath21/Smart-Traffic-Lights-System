namespace DetectionStore.Models.Requests;

public class RecordEmergencyVehicleRequest
{
    public Guid IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string? Type { get; set; }
    public int? PriorityLevel { get; set; }
}
