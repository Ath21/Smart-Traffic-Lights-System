using System;

namespace TrafficLightCacheData.Keys;

public static class TrafficLightCacheKeys
{
    public static string State(int intersection) => $"traffic_light:{intersection}:state";
    public static string Duration(int intersection) => $"traffic_light:{intersection}:duration";
    public static string LastUpdate(int intersection) => $"traffic_light:{intersection}:last_update";
    public static string Priority(int intersection) => $"traffic_light:{intersection}:priority";
    public static string QueueLength(int intersection) => $"traffic_light:{intersection}:queue_length";
}