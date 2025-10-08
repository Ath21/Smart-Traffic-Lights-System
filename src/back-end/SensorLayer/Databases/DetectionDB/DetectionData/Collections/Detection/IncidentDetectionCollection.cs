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
    public string? IncidentId { get; set; } 

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; } 

    [BsonElement("intersection")]
    public string? Intersection { get; set; }

    [BsonElement("reportedAt")]
    public DateTime ReportedAt { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }
}

/*

{
  "intersection_id": 3,
  "intersection": "Dytiki Pyli",
  "reportedAt": "2025-10-07T14:15:05Z",
  "description": "Minor collision detected between two vehicles on southbound lane."
}

*/
