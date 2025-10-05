using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

// Updated by : Detection Service
// Read by    : Sensor Service, Detection Service, Traffic Analytics Service
[BsonDiscriminator("public_transport_detections")]
[BsonIgnoreExtraElements]
public class PublicTransportDetectionCollection
{
    // unique detection ID
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PublicId { get; set; } = string.Empty; 

    // reference to intersection
    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; }

    // readable intersection name
    [BsonElement("intersection")]
    public string Intersection { get; set; } = string.Empty; 

    // detection time (UTC)
    [BsonElement("detectedAt")]
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    // transport line (e.g., "Bus 829")
    [BsonElement("line")]
    public string Line { get; set; } = string.Empty;
}
