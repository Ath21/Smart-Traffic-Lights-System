using System;

namespace SensorMessages;

// sensor.detection.{intersection}.{event}
public class SensorDetectionMessage
{
    public string Intersection { get; set; }
    public string Event { get; set; } // emergency, incident, public transport
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}