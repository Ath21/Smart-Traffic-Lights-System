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
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FailoverId { get; set; } = string.Empty;

    [BsonElement("correlation_id")]
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("layer")]
    public string Layer { get; set; } = string.Empty;

    [BsonElement("service")]
    public string Service { get; set; } = string.Empty;

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; } = 0;

    [BsonElement("intersection_name")]
    public string IntersectionName { get; set; } = string.Empty;

    [BsonElement("light_ids")]
    public List<int> LightId { get; set; } = new();

    [BsonElement("traffic_lights")]
    public List<string> TrafficLight { get; set; } = new();

    [BsonElement("context")]
    public string Context { get; set; } = string.Empty; // ApplyFailoverAsync, CachedStateMonitor, etc.

    [BsonElement("reason")]
    public string Reason { get; set; } = string.Empty; // Redis unavailable, network timeout, etc.

    [BsonElement("mode")]
    public string Mode { get; set; } = string.Empty; // CachedState, ManualMode, BlinkingYellow

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("metadata")]
    public BsonDocument Metadata { get; set; } = new();
}

/*

{
  "timestamp": "2025-10-07T11:15:30Z",
  "correlation_id": "b5b1d8f3-9812-4e26-87b7-53f92b27a666",
  "layer": "Traffic",
  "service": "Intersection Controller Service",
  "intersection_id": 5,
  "intersection_name": "Kentriki Pyli",
  "light_ids": [501, 502],
  "traffic_lights": ["kentriki-pyli501", "agiou-spyridonos502"],
  "context": "ApplyFailoverAsync",
  "reason": "Redis unavailable",
  "mode": "CachedState",
  "message": "Failover mode activated for intersection 'Kentriki Pyli'. Traffic lights operating using last known cached state.",
  "metadata": {
    "recovery_strategy": "Use previous phase durations from cache",
    "retry_attempts": "2",
    "intersection_status": "Degraded",
    "fallback_duration_sec": "45"
  }
}

*/
