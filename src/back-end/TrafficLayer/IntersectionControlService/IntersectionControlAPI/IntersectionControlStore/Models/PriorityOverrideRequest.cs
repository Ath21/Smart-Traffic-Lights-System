using System;

namespace IntersectionControlStore.Models;

public class PriorityOverrideRequest
{
    public bool PriorityEmergencyVehicle { get; set; }
    public bool PriorityPublicTransport { get; set; }
    public bool PriorityPedestrian { get; set; }
    public bool PriorityCyclist { get; set; }
    public int Duration { get; set; } // seconds
}

