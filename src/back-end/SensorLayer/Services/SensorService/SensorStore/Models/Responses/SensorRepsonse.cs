using System;

namespace SensorStore.Models.Responses;

public class SensorResponse
{
    public int IntersectionId { get; set; }
    public string IntersectionName { get; set; }
    public int VehicleCount { get; set; }
    public int PedestrianCount { get; set; }
    public int CyclistCount { get; set; }
    public DateTime Timestamp { get; set; }
}
