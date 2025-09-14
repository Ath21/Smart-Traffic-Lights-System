using System;

namespace LogMessages;

// log.failover.{service}.{context}
public record FailoverMessage(
    Guid LogId,
    string ServiceName,      // e.g., "Traffic Light Controller Service"
    string Context,          // could be "ekklhsia.west", "detection.cache", "analytics.db"
    string Reason,           // e.g., "RedisUnavailable", "DbTimeout", "ManualTest"
    string Mode,             // e.g., "BlinkingYellow", "AllRed", "FallbackCache", "SafeStop"
    DateTime Timestamp,
    object? Metadata         // flexible payload for extra details
);

