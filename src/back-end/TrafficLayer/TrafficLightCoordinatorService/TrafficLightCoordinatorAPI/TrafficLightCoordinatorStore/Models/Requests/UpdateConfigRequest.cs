using System;

namespace TrafficLightCoordinatorStore.Models.Requests;

public class UpdateConfigRequest
{
    public string Pattern { get; set; } = string.Empty;
}
