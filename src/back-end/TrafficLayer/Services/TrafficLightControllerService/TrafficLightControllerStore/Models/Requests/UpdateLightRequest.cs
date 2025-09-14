using System;

namespace TrafficLightControllerStore.Models.Requests;

public class UpdateLightRequest
{
    public string CurrentState { get; set; } = default!;
    public int? Duration { get; set; } // optional override duration (seconds)
    public string? Reason { get; set; } // optional reason for audit log
}