// sensor.detection.{intersection}.{event}
//
// {intersection} : agiou-spyridonos, anatoliki-pyli, dytiki-pyli, ekklisia, kentriki-pyli
// {event}        : emergency, incident, public-transport
//
// Published by : Detection Service
// Consumed by  : Intersection Controller Service, Traffic Light Analytics Service
namespace Messages.Sensor;

public class DetectionEventMessage 
{
  public int IntersectionId { get; set; }
  public string? IntersectionName { get; set; }
  public string? EventType { get; set; }
  public string? VehicleType { get; set; }
  public string? Direction { get; set; }
}

/*

###########
EMERGENCY VEHICLE DETECTION
###########

sensor.detection.agiou-spyridonos.emergency

{
  "CorrelationId": "c413af85-1d21-4ee1-8245-f65095812b0a",
  "Timestamp": "2025-10-07T14:10:22Z",
  "SourceServices": ["Detection Service"],
  "DestinationServices": ["Intersection Controller Service", "Traffic Analytics Service"],
  "IntersectionId": 2,
  "IntersectionName": "Agiou Spyridonos",
  "EventType": "emergency",
  "VehicleType": "ambulance",
  "Direction": "north",
  "Metadata": {
    "speed_kmh": "65.2",
    "signal_strength": "0.94"
  }
}

###########
PUBLIC TRANSPORT DETECTION
###########

sensor.detection.kentriki-pyli.public-transport

{
  "CorrelationId": "a12eab67-48bb-4b72-88a5-0e93e30a581a",
  "Timestamp": "2025-10-07T14:12:40Z",
  "SourceServices": ["Detection Service"],
  "DestinationServices": ["Intersection Controller Service", "Traffic Analytics Service"],
  "IntersectionId": 5,
  "IntersectionName": "Kentriki Pyli",
  "EventType": "public-transport",
  "VehicleType": "bus",
  "Direction": "east",
  "Metadata": {
    "line": "Bus829",
    "arrival_estimated_sec": "45",
  }
}

###########
INCIDENT DETECTION
###########

sensor.detection.dytiki-pyli.incident

{
  "CorrelationId": "f91d4bde-1eea-47b8-a41e-45e517f29bc3",
  "Timestamp": "2025-10-07T14:15:05Z",
  "SourceServices": ["Detection Service"],
  "DestinationServices": ["Intersection Controller Service", "Traffic Analytics Service"],
  "IntersectionId": 3,
  "IntersectionName": "Dytiki Pyli",
  "EventType": "incident",
  "VehicleType": null,
  "Direction": "south",
  "Metadata": {
    "description": "Minor collision detected between two vehicles on southbound lane.",
    "severity": "low"
  }
}

*/