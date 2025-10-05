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
    // unique record ID
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PedestrianId { get; set; } = string.Empty; 

    // reference to intersection
    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    // readable intersection name
    [BsonElement("intersection")]
    public string Intersection { get; set; } = string.Empty;

    // capture time (UTC)
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // total number of pedestrians detected
    [BsonElement("count")]
    public int Count { get; set; } = 0;
}
