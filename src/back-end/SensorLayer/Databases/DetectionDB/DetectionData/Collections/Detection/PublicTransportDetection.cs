using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

public class PublicTransportDetection
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

    [BsonElement("mode")]
    public string Mode { get; set; }
    
    [BsonElement("direction")]
    public string Direction { get; set; }
}
