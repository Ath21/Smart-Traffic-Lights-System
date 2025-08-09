using System;

namespace PublicTransportDetectionStore.Models;

public class PublicTransportDetectionCreateDto
{
    public Guid IntersectionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string RouteId { get; set; } = string.Empty;
}