using System;

namespace TrafficLightCoordinatorStore.Models;

public class PhaseDto
{
    public string Phase { get; set; } = string.Empty;
    public int Duration { get; set; }
}
