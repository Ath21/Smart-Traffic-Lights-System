using System;

namespace SensorMessages.SensorCount;

// sensor.count.{intersection}.pedestrian
public class PedestrianCountMessage
{
    public int IntersectionId { get; set; }
    public string IntersectionName { get; set; }
    public DateTime Timestamp { get; set; }
    public int Count { get; set; }
    public string Direction { get; set; }
}
