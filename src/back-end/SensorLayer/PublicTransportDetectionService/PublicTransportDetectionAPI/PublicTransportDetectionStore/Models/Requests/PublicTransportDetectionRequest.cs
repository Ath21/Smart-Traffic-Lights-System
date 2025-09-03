using System;

namespace PublicTransportDetectionStore.Models.Requests;
public class PublicTransportDetectionRequest
{
    public string RouteId { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
}
