using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

public class IncidentDetection
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } 

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    [BsonElement("intersection_name")]
    public string IntersectionName { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } 
    [BsonElement("severity")]
    public int Severity { get; set; } 

    [BsonElement("description")]
    public string Description { get; set; } 

    [BsonElement("direction")]
    public string Direction { get; set; } = "unknown";
}
