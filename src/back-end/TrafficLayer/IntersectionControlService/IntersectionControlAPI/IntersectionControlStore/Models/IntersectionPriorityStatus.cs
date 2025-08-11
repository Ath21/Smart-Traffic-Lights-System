using System;

namespace IntersectionControlStore.Models;

// Models/IntersectionPriorityStatus.cs
public class IntersectionPriorityStatus
{
    public Guid IntersectionId { get; init; }
    public bool PriorityEmergencyVehicle { get; set; }
    public bool PriorityPublicTransport { get; set; }
    public bool PriorityPedestrian { get; set; }
    public bool PriorityCyclist { get; set; }
    public DateTime UpdatedAt { get; set; }
    public CancellationTokenSource? OverrideCancellationTokenSource { get; set; }
}

