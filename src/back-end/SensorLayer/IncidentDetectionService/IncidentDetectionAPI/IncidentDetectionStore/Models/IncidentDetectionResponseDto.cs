using System;

namespace IncidentDetectionStore.Models;

public class IncidentDetectionResponseDto
{
    public Guid DetectionId { get; set; }
    public string Status { get; set; } = "Created";
}
