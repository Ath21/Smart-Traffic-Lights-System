using System;

namespace DetectionStore.Models.Dtos;

public class IncidentDto
{
    public int IntersectionId { get; set; }
    public string Type { get; set; } = default!;
    public int Severity { get; set; }
    public string Description { get; set; } = default!;
    public DateTime Timestamp { get; set; }
}