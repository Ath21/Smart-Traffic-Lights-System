using System;

namespace IncidentDetectionStore.Models.Requests;

public class IncidentDetectionRequest
{
    public string Description { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
}
