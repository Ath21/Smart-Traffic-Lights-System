using System;

namespace SensorMessages.SensorEvent;

// sensor.detection.{intersection}.public_transport
public class PublicTransportDetectedMessage
{
    public int IntersectionId { get; set; }
    public string IntersectionName { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Detected { get; set; }
    public string Mode { get; set; }
    public string Direction { get; set; }
}
