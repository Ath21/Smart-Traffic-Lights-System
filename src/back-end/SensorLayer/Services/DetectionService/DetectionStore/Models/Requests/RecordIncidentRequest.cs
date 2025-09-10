namespace DetectionStore.Models.Requests;

public class RecordIncidentRequest
{
    public Guid IntersectionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Severity { get; set; }
    public string Description { get; set; } = string.Empty;
}