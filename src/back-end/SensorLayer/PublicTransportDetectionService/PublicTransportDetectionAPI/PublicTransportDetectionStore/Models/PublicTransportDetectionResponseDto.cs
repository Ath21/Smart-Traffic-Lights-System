using System;

namespace PublicTransportDetectionStore.Models;

public class PublicTransportDetectionResponseDto
{
    public Guid DetectionId { get; set; }
    public string Status { get; set; } = string.Empty;
}