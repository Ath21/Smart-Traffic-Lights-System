namespace DetectionStore.Models.Dtos;

public class DetectionHistoryDto
{
    public DateTime Timestamp { get; set; }
    public EmergencyVehicleDto? EmergencyVehicle { get; set; }
    public PublicTransportDto? PublicTransport { get; set; }
    public IncidentDto? Incident { get; set; }
}