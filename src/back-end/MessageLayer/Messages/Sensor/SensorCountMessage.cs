// sensor.count.{intersection}.{type}
//
// {intersection} : agiou-spyridonos, anatoliki-pyli, dytiki-pyli, ekklisia, kentriki-pyli
// {type}         : vehicle, pedestrian, cyclist
//
// Published by : Sensor Service
// Consumed by  : Intersection Controller Service, Traffic Analytics Service
namespace Messages.Sensor;

public class SensorCountMessage : BaseMessage
{
  public string? CountType { get; set; }
  public int Count { get; set; }
  public double AverageSpeedKmh { get; set; }
  public double AverageWaitTimeSec { get; set; }
  public double FlowRate { get; set; }
  public Dictionary<string, int>? Breakdown { get; set; }
}

/*

###########
VEHICLE COUNT
###########

sensor.count.agioy-spyridonos.vehicle

{
  "CorrelationId": "f217ac02-6b8c-4cb8-9af2-f95b6ad7a532",
  "Timestamp": "2025-10-07T13:50:21Z",
  "SourceServices": ["Sensor Service"],
  "DestinationServices": ["Intersection Controller Service", "Traffic Analytics Service"],
  "IntersectionId": 2,
  "IntersectionName": "Agiou Spyridonos",
  "CountType": "Vehicle",
  "Intersection": "agiou-spyridonos",
  "Count": 48,
  "AverageSpeedKmh": 39.4,
  "AverageWaitTimeSec": 14.2,
  "FlowRate": 1.6,
  "Breakdown": {
    "car": 38,
    "bus": 4,
    "truck": 2,
    "motorcycle": 4
  }
}

###########
PEDESTRIAN COUNT
###########

sensor.count.dytiki-pyli.pedestrian

{
  "CorrelationId": "a3721e58-0d1c-4a56-8d47-2f58f3448cde",
  "Timestamp": "2025-10-07T13:55:12Z",
  "SourceServices": ["Sensor Service"],
  "DestinationServices": ["Intersection Controller Service", "Traffic Analytics Service"],
  "IntersectionId": 3,
  "IntersectionName": "Dytiki Pyli",
  "CountType": "Pedestrian",
  "Intersection": "dytiki-pyli",
  "Count": 22,
  "AverageSpeedKmh": 0,
  "AverageWaitTimeSec": 0,
  "FlowRate": 0
}


###########
CYCLIST COUNT
###########

sensor.count.kentriki-pyli.cyclist

{
  "CorrelationId": "bba25e7a-96e3-4f3a-a0bb-889dcf34572c",
  "Timestamp": "2025-10-07T13:52:10Z",
  "SourceServices": ["Sensor Service"],
  "DestinationServices": [ "Intersection Controller Service", "Traffic Analytics Service"],
  "IntersectionId": 5,
  "IntersectionName": "Kentriki Pyli",
  "CountType": "Cyclist",
  "Intersection": "kentriki-pyli",
  "Count": 6,
  "AverageSpeedKmh": 0,
  "AverageWaitTimeSec": 0,
  "FlowRate": 0.2
}

*/
