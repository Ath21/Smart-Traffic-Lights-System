using System;

namespace TrafficLightControllerStore.Models.Requests;

public class UpdateLightRequest
{
    public string CurrentState { get; set; } = string.Empty;
}
