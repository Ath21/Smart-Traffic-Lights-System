using System;

namespace TrafficLightCacheData.Keys;

public static class TrafficLightCacheKeys
{
    // Stores the current state of a traffic light (Red, Green, Yellow, BlinkingYellow, Off).
    // Value type : string — name of the current TrafficLightState enum.
    // Updated by : Traffic Light Controller Service
    // Read by    : Traffic Light Controller Service, Intersection Controller Service
    public static string State(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:state";

    // Stores the current duration (in seconds) for which the current light state
    // should remain active.
    // Value type : int — duration in seconds.
    // Updated by : Intersection Controller Service
    // Read by    : Traffic Light Controller Service, Intersection Controller Service
    public static string Duration(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:duration";

    // Stores the timestamp of the last state update for this traffic light.
    // Value type : string or DateTime serialized as string.
    // Updated by : Traffic Light Controller Service
    // Read by    : Traffic Light Controller Service, Intersection Controller Service
    public static string LastUpdate(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:last_update";

    // Stores the current priority level of the traffic light,
    // used to determine preemption by emergency or public transport vehicles or incidents.
    // Value type : int — higher value means higher priority.
    // Updated by : Intersection Controller Service
    // Read by    : Traffic Light Controller Service, Intersection Controller Service
    public static string Priority(int intersectionId, int lightId)
        => $"traffic_light:{intersectionId}:{lightId}:priority";
}
