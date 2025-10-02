using System;

namespace SensorMessages.SensorEvent;

// sensor.detection.{intersection}.incident
public class IncidentDetectedMessage
{
    public int IntersectionId { get; set; }
    public string IntersectionName { get; set; }
    public DateTime Timestamp { get; set; }
    public string Type { get; set; }
    public int Severity { get; set; }
    public string Description { get; set; }
    public string Direction { get; set; }
}
