using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DetectionData.Collections.Detection;

// Updated by : Detection Service
// Read by    : Sensor Service, Detection Service, Traffic Analytics Service
[BsonDiscriminator("emergency_vehicle_detections")]
[BsonIgnoreExtraElements]
public class EmergencyVehicleDetectionCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string EmergencyId { get; set; } = string.Empty;

    [BsonElement("intersection_id")]
    public int IntersectionId { get; set; } = 0;

    [BsonElement("intersection")]
    public string Intersection { get; set; } = string.Empty;

    [BsonElement("detectedAt")]
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("direction")]
    public string Direction { get; set; } = string.Empty;

    public string EmergencyVehicleType { get; set; } = string.Empty;
}

/*

{
  "intersection_id": 2,
  "intersection": "Agiou Spyridonos",
  "detectedAt": "2025-10-07T14:10:22Z",
  "direction": "north",
  "emergency_vehicle_type": "ambulance"
}

*/
