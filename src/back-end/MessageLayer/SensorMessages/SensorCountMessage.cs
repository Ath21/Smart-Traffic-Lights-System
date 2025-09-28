using System;

namespace SensorMessages;

// sensor.count.{intersection}.{type}
public class SensorCountMessage
{
    public string Intersection { get; set; }
    public string Type { get; set; } // vehicle, pedestrian, cyclist
    public int Count { get; set; }
    public DateTime Timestamp { get; set; }
}