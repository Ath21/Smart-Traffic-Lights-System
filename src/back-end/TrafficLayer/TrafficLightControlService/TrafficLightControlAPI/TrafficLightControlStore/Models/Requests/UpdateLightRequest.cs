using System;

namespace TrafficLightControlStore.Models.Requests;

public class UpdateLightRequest
{
    public string CurrentState { get; set; } = string.Empty;
}
