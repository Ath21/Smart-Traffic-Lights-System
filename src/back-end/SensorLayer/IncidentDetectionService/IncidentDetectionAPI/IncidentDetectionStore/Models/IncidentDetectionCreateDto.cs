using System;

namespace IncidentDetectionStore.Models;

public class IncidentDetectionCreateDto
{
    public Guid IntersectionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
