namespace DetectionStore.Models.Requests;

public class RecordPublicTransportRequest
{
    public Guid IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string? Mode { get; set; }
    public string? RouteId { get; set; }
}
