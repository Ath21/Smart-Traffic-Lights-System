using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

// Updated by : Log Service
// Read by    : Log Service
[BsonDiscriminator("failover_logs")]
[BsonIgnoreExtraElements]
public class FailoverLogCollection
{
    // Unique identifier for the failover log entry
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FailoverId { get; set; } = string.Empty;

    // Timestamp when failover was triggered (UTC)
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Layer affected by failover (Traffic, Sensor, User)
    [BsonElement("layer")]
    public string Layer { get; set; } = string.Empty;

    // Service that entered failover (e.g. "TrafficLightControllerService")
    [BsonElement("service")]
    public string Service { get; set; } = string.Empty;

    // Context of failure (Database, Network, Service, etc.)
    [BsonElement("context")]
    public string Context { get; set; } = string.Empty;

    // Cause of failover (e.g. "RedisUnavailable", "TrafficLightFailure")
    [BsonElement("reason")]
    public string Reason { get; set; } = string.Empty;

    // Mode activated during failover (e.g. "BlinkingYellow", "ManualControl")
    [BsonElement("mode")]
    public string Mode { get; set; } = "BlinkingYellow";

    // Additional details (stack trace, affected intersections)
    [BsonElement("metadata")]
    public BsonDocument Metadata { get; set; } = new();
}