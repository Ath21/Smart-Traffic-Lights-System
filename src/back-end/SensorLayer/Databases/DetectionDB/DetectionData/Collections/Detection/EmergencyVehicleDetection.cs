using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

public class EmergencyVehicleDetection
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } 

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    [BsonElement("intersection_name")]
    public string IntersectionName { get; set; } 

    [BsonElement("detected")]
    public bool Detected { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } 

    [BsonElement("priority_level")]
    public int PriorityLevel { get; set; }
    
    [BsonElement("direction")]
    public string Direction { get; set; } = "unknown";
}
