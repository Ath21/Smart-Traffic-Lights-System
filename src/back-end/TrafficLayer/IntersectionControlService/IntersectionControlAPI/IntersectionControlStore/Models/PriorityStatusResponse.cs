using System;

namespace IntersectionControlStore.Models;

public class PriorityStatusResponse
{
    public Guid IntersectionId { get; set; }
    public bool PriorityEmergencyVehicle { get; set; }
    public bool PriorityPublicTransport { get; set; }
    public bool PriorityPedestrian { get; set; }
    public bool PriorityCyclist { get; set; }
    public DateTime UpdatedAt { get; set; }
}
