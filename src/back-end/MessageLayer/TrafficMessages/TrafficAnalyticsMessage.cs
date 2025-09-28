using System;

namespace TrafficMessages;

// traffic.analytics.{intersection}.{metric}
public class TrafficAnalyticsMessage
{
    public string Intersection { get; set; }
    public string Metric { get; set; } // congestion, avg_speed, incidents
    public string Value { get; set; }
    public DateTime Timestamp { get; set; }
}

