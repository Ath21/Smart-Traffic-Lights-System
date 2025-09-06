using System;

namespace DetectionCacheData.Entities;

public class DetectionCache
{
    public Guid IntersectionId { get; set; }
    public int VehicleCount { get; set; }
    public int PedestrianCount { get; set; }
    public int CyclistCount { get; set; }
    public bool EmergencyDetected { get; set; }
    public bool PublicTransportDetected { get; set; }
    public string? LastIncident { get; set; }
    public DateTime LastUpdated { get; set; }
}