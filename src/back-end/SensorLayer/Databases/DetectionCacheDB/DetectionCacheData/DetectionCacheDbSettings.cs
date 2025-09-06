using System;

namespace DetectionCacheData;

public class DetectionCacheDbSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }
    public int Database { get; set; }

    public string KeyPrefix_VehicleCount { get; set; }
    public string KeyPrefix_PedestrianCount { get; set; }
    public string KeyPrefix_CyclistCount { get; set; }
    public string KeyPrefix_EmergencyDetected { get; set; }
    public string KeyPrefix_PublicTransportDetected { get; set; }
    public string KeyPrefix_IncidentDetected { get; set; }
}