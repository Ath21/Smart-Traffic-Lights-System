using System;

namespace TrafficMessages;

// traffic.light.update.{intersection}
public class TrafficLightUpdateMessage
{
    public string Intersection { get; set; }
    public Dictionary<string, string> Lights { get; set; } // lightId -> state
    public DateTime Timestamp { get; set; }
}
