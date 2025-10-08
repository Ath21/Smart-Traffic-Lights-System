using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Count;

// Updated by : Sensor Service
// Read by    : Sensor Service, Detection Service, Traffic Analytics Service
[BsonDiscriminator("cyclist_count")]
[BsonIgnoreExtraElements]
public class CyclistCountCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? CyclistId { get; set; } 

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    [BsonElement("intersection")]
    public string? Intersection { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } 

    [BsonElement("count")]
    public int Count { get; set; }
}

/*

{
  "intersection_id": 5,
  "intersection": "Kentriki Pyli",
  "timestamp": "2025-10-07T13:52:10Z",
  "count": 6
}

*/
