namespace DetectionStore.Models.Responses;

public class DetectionEventResponse
{
    public int IntersectionId { get; set; }
    public string EventType { get; set; }
    public string? SubType { get; set; }
    public int? PriorityLevel { get; set; }
    public int? Severity { get; set; }
    public string? Description { get; set; }
    public bool Detected { get; set; }
    public DateTime Timestamp { get; set; }
}
