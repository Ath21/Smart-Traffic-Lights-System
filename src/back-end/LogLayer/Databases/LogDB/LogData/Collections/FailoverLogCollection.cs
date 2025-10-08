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
    public string? FailoverId { get; set; }

    [BsonElement("correlation_id")]
    public Guid CorrelationId { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("layer")]
    public string? Layer { get; set; }

    [BsonElement("service")]
    public string? Service { get; set; }

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    [BsonElement("intersection_name")]
    public string? IntersectionName { get; set; }

    [BsonElement("light_ids")]
    public List<int>? LightId { get; set; }

    [BsonElement("traffic_lights")]
    public List<string>? TrafficLight { get; set; }

    [BsonElement("context")]
    public string? Context { get; set; } // ApplyFailoverAsync, CachedStateMonitor, etc.

    [BsonElement("reason")]
    public string? Reason { get; set; } // Redis unavailable, network timeout, etc.

    [BsonElement("mode")]
    public string? Mode { get; set; } // CachedState, ManualMode, BlinkingYellow

    [BsonElement("message")]
    public string? Message { get; set; }

    [BsonElement("metadata")]
    public BsonDocument? Metadata { get; set; }
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
