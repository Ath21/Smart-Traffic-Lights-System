using System;

namespace TrafficLightCacheData;

public class TrafficLightCacheDbSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }
    public int Database { get; set; }

    public string KeyPrefix_State { get; set; }
    public string KeyPrefix_Duration { get; set; }
    public string KeyPrefix_LastUpdate { get; set; }
    public string KeyPrefix_Priority { get; set; }
    public string KeyPrefix_QueueLength { get; set; }
}
