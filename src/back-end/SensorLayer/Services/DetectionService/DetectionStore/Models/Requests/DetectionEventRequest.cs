namespace DetectionStore.Models.Requests;

public class DetectionEventRequest
{
    public int IntersectionId { get; set; }
    public string IntersectionName { get; set; }
    public string EventType { get; set; } // emergency, public_transport, incident
    public string? SubType { get; set; }  // ambulance, bus, collision, etc.
    public int? PriorityLevel { get; set; }
    public int? Severity { get; set; }
    public string? Description { get; set; }
    public string Direction { get; set; } // north, south, east, west
    public bool Detected { get; set; }
}
