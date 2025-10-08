using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Count;

// Updated by : Sensor Service
// Read by    : Sensor Service, Detection Service, Traffic Analytics Service
[BsonDiscriminator("pedestrian_count")]
[BsonIgnoreExtraElements]
public class PedestrianCountCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? PedestrianId { get; set; }

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
  "intersection_id": 3,
  "intersection": "Dytiki Pyli",
  "timestamp": "2025-10-07T13:55:12Z",
  "count": 22
}

*/
