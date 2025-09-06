using System;

namespace TrafficLightCoordinatorStore.Models.Dtos;

public class CongestionDto
{
    public Guid IntersectionId { get; set; }
    public double CongestionLevel { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string AppliedPattern { get; set; } = string.Empty;
    public DateTimeOffset AppliedAt { get; set; }
}
