namespace DetectionStore.Models.Responses;


public class PublicTransportResponse
{
    public Guid IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string? Mode { get; set; }
    public string? RouteId { get; set; }
    public DateTime Timestamp { get; set; }
}