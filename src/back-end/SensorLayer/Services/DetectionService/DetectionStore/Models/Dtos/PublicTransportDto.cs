using System;

namespace DetectionStore.Models.Dtos;

public class PublicTransportDto
{
    public int IntersectionId { get; set; }
    public bool Detected { get; set; }
    public string Mode { get; set; } = default!;
    public string RouteId { get; set; } = default!;
    public DateTime Timestamp { get; set; }
}
