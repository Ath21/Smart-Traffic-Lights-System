using System;

namespace TrafficLightCoordinatorStore.Models.Dtos;

public class PriorityDto
{
    public Guid IntersectionId { get; set; }
    public string PriorityType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string AppliedPattern { get; set; } = string.Empty;
    public DateTimeOffset AppliedAt { get; set; }
}
