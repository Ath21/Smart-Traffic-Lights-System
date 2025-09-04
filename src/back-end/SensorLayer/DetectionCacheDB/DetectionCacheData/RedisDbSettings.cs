using System;

namespace DetectionCacheData;

public class RedisDbSettings
{
    public string Host { get; set; } 
    public int Port { get; set; } 
    public string Password { get; set; }
    public int Database { get; set; }
}