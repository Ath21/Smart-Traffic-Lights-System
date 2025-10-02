using System;

namespace DetectionCacheData.Keys;

public static class DetectionCacheKeys
{
    public static string VehicleCount(int intersection) => $"sensor:{intersection}:vehicle_count";
    public static string PedestrianCount(int intersection) => $"sensor:{intersection}:pedestrian_count";
    public static string CyclistCount(int intersection) => $"sensor:{intersection}:cyclist_count";
    public static string EmergencyDetected(int intersection) => $"sensor:{intersection}:emergency_detected";
    public static string PublicTransportDetected(int intersection) => $"sensor:{intersection}:public_transport_detected";
    public static string IncidentDetected(int intersection) => $"sensor:{intersection}:incident_detected";
}
