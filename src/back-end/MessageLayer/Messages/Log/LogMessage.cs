// log.{layer}.{service}.{type}
//
// {layer}   : sensor, traffic, user
// {service} : sensor, detection, 
//             intersection-controller, traffic-light-controller, traffic-analytics, traffic-light-coordinator, 
//             user, notification
//
// Published by : Sensor Service, Detection Service, 
//                Intersection Controller Service, Traffic Light Controller Service, Traffic Analytics Service, Traffic Light Coordinator Service,
//                User Service, Notification Service
// Consumed by  : Log Service
namespace Messages.Log;

public class LogMessage : BaseMessage
{
  public string? Layer { get; set; }
  public string? LogType { get; set; }
  public List<int>? LightId { get; set; }
  public List<string>? TrafficLight { get; set; }
  public string? Action { get; set; }
  public string? Message { get; set; }
}

/*

############
AUDIT LOG
############

log.traffic.intersection-controller.audit

{
  "CorrelationId": "a4e7c9d1-0bb4-4f73-b8f3-b8a83032a30f",
  "Timestamp": "2025-10-06T14:21:03Z",
  "SourceServices": ["Intersection Controller Service"],
  "DestinationServices": ["Log Service"],
  "IntersectionId": 2,
  "IntersectionName": "Agiou Spyridonos",
  "Layer": "Traffic",
  "LogType": "Audit",
  "LightId": [201, 202],
  "TrafficLight": ["agiou-spyridonos201", "dimitasanas202"],
  "Action": "ModeSwitch",
  "Message": "Intersection 'Agiou Spyridonos' switched from Normal to Congestion mode.",
  "Metadata": {
    "previous_mode": "Normal",
    "new_mode": "Congestion",
    "congestion_index": "0.84"
  }
}

############
ERROR LOG
############

log.traffic.traffic-light-controller.error

{
  "CorrelationId": "e41e3e67-8749-41f3-8a3b-9a22a091f9bb",
  "Timestamp": "2025-10-07T11:15:22Z",

  "SourceServices": ["Traffic Light Controller Service"],
  "DestinationServices": ["Log Service"],

  "IntersectionId": 3,
  "IntersectionName": "Dytiki Pyli",

  "Layer": "Traffic",
  "LogType": "Error",

  "LightId": [301],
  "TrafficLight": ["dytiki-pyli301"],

  "Action": "RedisFetchState",
  "Message": "Failed to retrieve current phase durations from Redis for lights 301 at intersection 'Dytiki Pyli'.",

  "Metadata": {
    "exception_type": "RedisConnectionException",
    "exception_message": "No connection available to Redis server at trafficlightcachedb:6379",
    "retry_count": "3",
    "component": "RedisStateFetcher",
    "duration_default_sec": "60"
  }
}

############
FAILOVER LOG
############

log.traffic.intersection-controller.failover

{
  "CorrelationId": "b5b1d8f3-9812-4e26-87b7-53f92b27a666",
  "Timestamp": "2025-10-07T11:15:30Z",

  "SourceServices": ["Intersection Controller Service"],
  "DestinationServices": ["Log Service"],

  "IntersectionId": 5,
  "IntersectionName": "Kentriki Pyli",

  "Layer": "Traffic",
  "LogType": "Failover",

  "LightId": [501, 502],
  "TrafficLight": ["kentriki-pyli501", "agiou-spyridonos502"],

  "Action": "ApplyFailoverAsync",
  "Message": "Failover mode activated for intersection 'Kentriki Pyli'. Traffic lights operating using last known cached state.",

  "Metadata": {
    "reason": "Redis unavailable",
    "failover_mode": "CachedState",
    "recovery_strategy": "Use previous phase durations from cache",
    "intersection_status": "Degraded",
    "retry_attempts": "2",
    "fallback_duration_sec": "45"
  }
}

*/