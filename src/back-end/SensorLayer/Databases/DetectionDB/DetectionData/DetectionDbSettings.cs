namespace DetectionData;

public class DetectionDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }

    // Collections
    public string VehicleCountCollection { get; set; }
    public string PedestrianCountCollection { get; set; }
    public string CyclistCountCollection { get; set; }

    public string EmergencyVehicleCollection { get; set; }
    public string IncidentCollection { get; set; }
    public string PublicTransportCollection { get; set; }
}
