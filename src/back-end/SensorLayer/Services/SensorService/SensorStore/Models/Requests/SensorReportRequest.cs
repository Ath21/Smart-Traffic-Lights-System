using System;

namespace SensorStore.Models.Requests;

public class SensorReportRequest
{
    public int IntersectionId { get; set; }
    public string IntersectionName { get; set; }
    public int VehicleCount { get; set; }
    public int PedestrianCount { get; set; }
    public int CyclistCount { get; set; }
    public string Direction { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
