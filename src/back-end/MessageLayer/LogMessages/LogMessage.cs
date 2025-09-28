using System;

namespace LogMessages;

// log.{layer}.{service}.{type}
public class LogMessage
{
    public string Layer { get; set; } // sensor, traffic, user, log
    public string Service { get; set; }
    public string Type { get; set; } // audit, error, failover
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

