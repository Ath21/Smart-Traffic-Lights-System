using System;

namespace TrafficMessages;

// traffic.light.control.{intersection}.{light}
public class TrafficLightControlMessage
{
    public string Intersection { get; set; }
    public string Light { get; set; }
    public string State { get; set; } // green, red, yellow
    public int Duration { get; set; }
    public DateTime Timestamp { get; set; }
}
