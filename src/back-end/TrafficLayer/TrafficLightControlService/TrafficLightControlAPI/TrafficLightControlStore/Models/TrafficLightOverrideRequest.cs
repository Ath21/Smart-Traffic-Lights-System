using System;

namespace TrafficLightControlStore.Models;

public class TrafficLightOverrideRequest
{
    public string State { get; set; } // "red", "yellow", "green"
    public int Duration { get; set; } // in seconds
}

