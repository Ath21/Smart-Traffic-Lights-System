using System;

namespace TrafficLightCoordinatorStore.Models.Requests;

public class PriorityOverrideRequest
{
    public Guid IntersectionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}