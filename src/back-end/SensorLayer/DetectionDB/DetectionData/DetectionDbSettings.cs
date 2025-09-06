using System;

namespace DetectionData;

public class DetectionDbSettings
{
    public string ConnectionString { get; set; }
    public string Database { get; set; }
    public string VehicleCollection { get; set; }
    public string PedestrianCollection { get; set; }
    public string CyclistCollection { get; set; }
    public string EmergencyCollection { get; set; }
    public string PublicTransportCollection { get; set; }
    public string IncidentCollection { get; set; }
}
