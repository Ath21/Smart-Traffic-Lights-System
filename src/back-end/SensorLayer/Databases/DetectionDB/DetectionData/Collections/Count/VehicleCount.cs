using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Count;

public class VehicleCount
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    [BsonElement("lane_id")]
    public string LaneId { get; set; }

    [BsonElement("count")]
    public int Count { get; set; }
}