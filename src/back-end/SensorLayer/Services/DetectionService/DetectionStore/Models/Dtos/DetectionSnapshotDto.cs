namespace DetectionStore.Models.Dtos;

public class DetectionSnapshotDto
{
    public Guid IntersectionId { get; set; }
    public EmergencyVehicleDto? EmergencyVehicle { get; set; }
    public PublicTransportDto? PublicTransport { get; set; }
    public IncidentDto? Incident { get; set; }
    public DateTime LastUpdated { get; set; }
}