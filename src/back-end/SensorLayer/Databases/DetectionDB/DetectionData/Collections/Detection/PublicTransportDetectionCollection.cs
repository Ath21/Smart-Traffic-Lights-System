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
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PublicId { get; set; } = string.Empty;

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; } = 0;

    [BsonElement("intersection_name")]
    public string IntersectionName { get; set; } = string.Empty;

    [BsonElement("line_name")]
    public string LineName { get; set; } = string.Empty;

    [BsonElement("detectedAt")]
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("line")]
    public string Line { get; set; } = string.Empty; // Bus829, Bus703, Bus831, Bus891
}

/*

{
  "intersection_id": 5,
  "intersection_name": "Kentriki Pyli",
  "line_name": "Bus829",
  "detectedAt": "2025-10-07T14:12:40Z",
  "line": "Bus829"
}

*/
