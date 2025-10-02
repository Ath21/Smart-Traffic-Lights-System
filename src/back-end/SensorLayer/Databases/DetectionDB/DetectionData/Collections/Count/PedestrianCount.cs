using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Count;

public class PedestrianCount
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    [BsonElement("intersection_name")]
    public string IntersectionName { get; set; } 

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } 

    [BsonElement("count")]
    public int Count { get; set; }

    [BsonElement("direction")]
    public string Direction { get; set; }
}