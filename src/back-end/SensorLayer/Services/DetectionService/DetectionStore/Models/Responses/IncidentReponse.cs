namespace DetectionStore.Models.Responses;

public class IncidentResponse
{
    public Guid IntersectionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}