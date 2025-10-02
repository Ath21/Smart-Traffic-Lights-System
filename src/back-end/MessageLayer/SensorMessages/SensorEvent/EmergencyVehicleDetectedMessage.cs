using System;

namespace SensorMessages.SensorEvent;

// sensor.detection.{intersection}.emergency_vehicle
public class EmergencyVehicleDetectedMessage
{
    public int IntersectionId { get; set; }
    public string IntersectionName { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Detected { get; set; }
    public string Type { get; set; }
    public int PriorityLevel { get; set; }
    public string Direction { get; set; }
}