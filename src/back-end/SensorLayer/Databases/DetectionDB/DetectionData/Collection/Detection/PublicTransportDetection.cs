using System;

namespace DetectionData.Collection.Detection;

public class PublicTransportDetection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
    public Guid IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string Mode { get; set; } = string.Empty;
    public string? RouteId { get; set; }
}
