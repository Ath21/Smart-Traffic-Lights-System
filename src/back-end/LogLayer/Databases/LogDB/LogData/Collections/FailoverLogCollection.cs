using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LogData.Collections;

// Updated by : Log Service
// Read by    : Log Service
public class FailoverLogCollection : BaseLogCollection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? FailoverId { get; set; }
}

/*

{
  "timestamp": "2025-10-06T14:21:03Z",
  "correlation_id": "a4e7c9d1-0bb4-4f73-b8f3-b8a83032a30f",
  "layer": "Traffic",
  "service": "Intersection Controller Service",
  "context": "ModeSwitch",
  "reason": "Lost connection to Central System",
  "mode": "FAILSAFE",
  "action": "SwitchToFailsafe",
  "message": "Intersection switched to FAILSAFE mode due to lost connection to Central System.",
  "metadata": {
    "intersection_id": 5,
    "intersection_name": "Kentriki Pyli",
    "light_ids": [501, 502, 503],
    "traffic_lights": ["kentriki-pyli501", "kentriki-pyli502", "kentriki-pyli503"]
  }
}

*/
