using System;

namespace TrafficMessages;

// priority.detection.{intersection}.{event}
public class PriorityDetectionMessage
{
    public string Intersection { get; set; }
    public string Event { get; set; } // emergency, incident, public transport
    public int PriorityLevel { get; set; }
    public DateTime Timestamp { get; set; }
}
