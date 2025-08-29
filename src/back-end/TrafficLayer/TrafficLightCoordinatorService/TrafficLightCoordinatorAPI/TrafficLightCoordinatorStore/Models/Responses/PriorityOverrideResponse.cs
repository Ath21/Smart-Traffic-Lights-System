using System;

namespace TrafficLightCoordinatorStore.Models.Responses;

public class PriorityOverrideResponse
{
    public Guid IntersectionId { get; set; }
    public string AppliedType { get; set; } = string.Empty;
    public string AppliedPattern { get; set; } = string.Empty;
    public DateTimeOffset AppliedAt { get; set; }
}
