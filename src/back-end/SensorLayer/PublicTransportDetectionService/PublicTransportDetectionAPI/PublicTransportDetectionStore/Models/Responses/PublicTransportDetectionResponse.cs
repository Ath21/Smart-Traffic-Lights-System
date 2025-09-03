using System;

namespace PublicTransportDetectionStore.Models.Responses;

public class PublicTransportDetectionResponse
{
    public Guid DetectionId { get; set; }
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string RouteId { get; set; } = string.Empty;
}
