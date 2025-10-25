using System;

namespace TrafficLightCoordinatorStore.Models;

public class ApplyModeRequest
{
    public int IntersectionId { get; set; }
    public string Mode { get; set; } = string.Empty;
}
