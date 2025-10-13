using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

// Updated by : Log Service
// Read by    : Log Service
// Updated by : Log Service
// Read by    : Log Service
public class AuditLogCollection : BaseLogCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? AuditId { get; set; }
}
/*

{
  "timestamp": "2025-10-06T14:21:03Z",
  "correlation_id": "a4e7c9d1-0bb4-4f73-b8f3-b8a83032a30f",
  "layer": "Traffic",
  "service": "Intersection Controller Service",
  "action": "PhaseChange",
  "message": "Traffic light 'kentriki-pyli501' changed to GREEN.",
  "metadata": {
    "intersection_id": 5,
    "intersection_name": "Kentriki Pyli",
    "light_id": 501,
    "traffic_light": "kentriki-pyli501",
    "from_phase": "YELLOW",
    "to_phase": "GREEN"
  }
}

*/  