using System;

namespace SensorStore.Models.Dtos;

public class SensorSnapshotDto
{
    public int IntersectionId { get; set; }
    public string IntersectionName { get; set; }
    public int VehicleCount { get; set; }
    public int PedestrianCount { get; set; }
    public int CyclistCount { get; set; }
    public DateTime LastUpdated { get; set; }
}
