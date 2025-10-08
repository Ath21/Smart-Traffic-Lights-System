using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

// Updated by : Log Service
// Read by    : Log Service
[BsonDiscriminator("error_logs")]
[BsonIgnoreExtraElements]
public class ErrorLogCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ErrorId { get; set; }

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

    [BsonElement("error_type")]
    public string? ErrorType { get; set; } // e.g. 500 Internal Server Error

    [BsonElement("action")]
    public string? Action { get; set; } // RedisFetchState, MongoWrite, etc.

    [BsonElement("message")]
    public string? Message { get; set; }

    [BsonElement("metadata")]
    public BsonDocument? Metadata { get; set; }
}

/*

{
  "timestamp": "2025-10-07T11:15:22Z",
  "correlation_id": "e41e3e67-8749-41f3-8a3b-9a22a091f9bb",
  "layer": "Traffic",
  "service": "Traffic Light Controller Service",
  "intersection_id": 3,
  "intersection_name": "Dytiki Pyli",
  "light_ids": [301],
  "traffic_lights": ["dytiki-pyli301"],
  "error_type": "500 Internal Server Error",
  "action": "RedisFetchState",
  "message": "Failed to retrieve current phase durations from Redis for light 301 at intersection 'Dytiki Pyli'.",
  "metadata": {
    "exception_type": "RedisConnectionException",
    "retry_count": "3",
    "component": "RedisStateFetcher"
  }
}

*/
