using System;

namespace DetectionStore.Models.Responses;

public class DetectionHistoryResponse
{
    public DateTime Timestamp { get; set; }
    public EmergencyVehicleResponse? EmergencyVehicle { get; set; }
    public PublicTransportResponse? PublicTransport { get; set; }
    public IncidentResponse? Incident { get; set; }
}
