namespace TrafficLightCacheData;

public class TrafficLightCacheDbSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? Password { get; set; }
    public int Database { get; set; }

    // Optional key prefixes (if you want config-driven customization)
    public string KeyPrefix_State { get; set; } = "traffic_light:{intersection}:state";
    public string KeyPrefix_Duration { get; set; } = "traffic_light:{intersection}:duration";
    public string KeyPrefix_LastUpdate { get; set; } = "traffic_light:{intersection}:last_update";
    public string KeyPrefix_Priority { get; set; } = "traffic_light:{intersection}:priority";
    public string KeyPrefix_QueueLength { get; set; } = "traffic_light:{intersection}:queue_length";
}
