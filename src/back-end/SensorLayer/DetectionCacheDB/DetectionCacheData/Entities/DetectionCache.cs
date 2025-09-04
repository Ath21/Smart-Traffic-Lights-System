using System;

namespace DetectionCacheData.Entities;

public class DetectionCache
{
    public int IntersectionId { get; set; }
    public int VehicleCount { get; set; }
    public int PedestrianCount { get; set; }
    public int CyclistCount { get; set; }
    public bool EmergencyDetected { get; set; }
    public bool PublicTransportDetected { get; set; }
    public string Incident { get; set; } // raw JSON {"type":"collision","severity":2}
}