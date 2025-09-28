using System;

namespace TrafficMessages;

// priority.count.{intersection}.{type}
public class PriorityCountMessage
{
    public string Intersection { get; set; }
    public string Type { get; set; } // vehicle, pedestrian, bicycle
    public int Count { get; set; }
    public DateTime Timestamp { get; set; }
}
