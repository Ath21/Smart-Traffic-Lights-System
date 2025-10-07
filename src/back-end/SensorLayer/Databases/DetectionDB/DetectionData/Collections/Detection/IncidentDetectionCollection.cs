using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

// Updated by : Detection Service
// Read by    : Sensor Service, Detection Service, Traffic Analytics Service
[BsonDiscriminator("incident_detections")]
[BsonIgnoreExtraElements]
public class IncidentDetectionCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string IncidentId { get; set; } = string.Empty;

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; } = 0;

    [BsonElement("intersection")]
    public string Intersection { get; set; } = string.Empty;

    [BsonElement("reportedAt")]
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;
}

/*

{
  "intersection_id": 3,
  "intersection": "Dytiki Pyli",
  "reportedAt": "2025-10-07T14:15:05Z",
  "description": "Minor collision detected between two vehicles on southbound lane."
}

*/
