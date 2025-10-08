using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

// Updated by : Log Service
// Read by    : Log Service
[BsonDiscriminator("audit_logs")]
[BsonIgnoreExtraElements]
public class AuditLogCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? AuditId { get; set; }

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

    [BsonElement("action")]
    public string? Action { get; set; } // ModeSwitch, PhaseChange, etc.

    [BsonElement("message")]
    public string? Message { get; set; }

    [BsonElement("metadata")]
    public BsonDocument? Metadata { get; set; }
}

/*

{
  "timestamp": "2025-10-06T14:21:03Z",
  "correlation_id": "a4e7c9d1-0bb4-4f73-b8f3-b8a83032a30f",
  "layer": "Traffic",
  "service": "Intersection Controller Service",
  "intersection_id": 2,
  "intersection_name": "Agiou Spyridonos",
  "light_ids": [201, 202],
  "traffic_lights": ["agiou-spyridonos201", "dimitasanas202"],
  "action": "ModeSwitch",
  "message": "Intersection 'Agiou Spyridonos' switched from Normal to Congestion mode.",
  "metadata": {
    "previous_mode": "Normal",
    "new_mode": "Congestion",
    "congestion_index": "0.84"
  }
}

*/
