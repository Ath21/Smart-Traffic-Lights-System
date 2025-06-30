using System;

namespace TrafficDataAnalyticsData.Redis;

public class RedisDbSettings
{
    public string ConnectionString { get; set; }
    public string Host { get; set; } 
    public int Port { get; set; }
}
