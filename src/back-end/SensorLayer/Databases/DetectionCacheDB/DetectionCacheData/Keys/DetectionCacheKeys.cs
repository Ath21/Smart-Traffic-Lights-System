using System;

namespace DetectionCacheData.Keys;

public static class DetectionCacheKeys
{
    // Stores the latest total vehicle count detected at a specific intersection.
    // Value type : int — number of vehicles.
    // Updated by : Sensor Service
    // Read by    : Sensor Service, Intersection Controller Service
    public static string VehicleCount(int intersectionId)
        => $"sensor:{intersectionId}:vehicle_count";

    // Stores the latest total pedestrian count detected at a specific intersection.
    // Value type : int — number of pedestrians.
    // Updated by : Sensor Service
    // Read by    : Sensor Service, Intersection Controller Service
    public static string PedestrianCount(int intersectionId)
        => $"sensor:{intersectionId}:pedestrian_count";

    // Stores the latest total cyclist count detected at a specific intersection.
    // Value type : int — number of cyclists.
    // Updated by : Sensor Service
    // Read by    : Sensor Service, Intersection Con

    // Stores a flag indicating whether a public transport vehicle
    // (bus, tram) has been detected at the intersection.
    // Value type : bool — true if public transport detected.
    // Updated by : Detection Service
    // Read by    : Detection Service, Intersection Controller Servicetroller Service
    public static string CyclistCount(int intersectionId)
        => $"sensor:{intersectionId}:cyclist_count";

    // Stores a flag indicating whether an emergency vehicle
    // has been detected near the intersection.
    // Value type : bool — true if emergency vehicle detected.
    // Updated by : Detection Service
    // Read by    : Detection Service, Intersection Controller Service
    public static string EmergencyDetected(int intersectionId)
        => $"detection:{intersectionId}:emergency_vehicle_detected";
    public static string PublicTransportDetected(int intersectionId)
        => $"detection:{intersectionId}:public_transport_detected";

    // Stores a flag indicating whether an incident (accident, obstacle, etc.)
    // has been detected at or near the intersection.
    // Value type : bool — true if an incident is currently active.
    // Updated by : Detection Service
    // Read by    : Detection Service, Intersection Controller Service
    public static string IncidentDetected(int intersectionId)
        => $"detection:{intersectionId}:incident_detected";
}
