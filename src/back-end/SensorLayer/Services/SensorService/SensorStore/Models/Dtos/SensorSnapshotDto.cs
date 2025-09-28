using System;

namespace SensorStore.Models.Dtos;

public class SensorSnapshotDto
{
    public int IntersectionId { get; set; }
    public int VehicleCount { get; set; }
    public int PedestrianCount { get; set; }
    public int CyclistCount { get; set; }
    public bool EmergencyDetected { get; set; }
    public bool PublicTransportDetected { get; set; }
    public string? LastIncident { get; set; }
    public float AvgSpeed { get; set; }
    public DateTime LastUpdated { get; set; }
}
