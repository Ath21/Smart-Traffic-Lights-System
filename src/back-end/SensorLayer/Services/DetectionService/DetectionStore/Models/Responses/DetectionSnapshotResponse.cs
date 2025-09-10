using System;

namespace DetectionStore.Models.Responses;

public class DetectionSnapshotResponse
{
    public Guid IntersectionId { get; set; }
    public EmergencyVehicleResponse? EmergencyVehicle { get; set; }
    public PublicTransportResponse? PublicTransport { get; set; }
    public IncidentResponse? Incident { get; set; }
    public DateTime LastUpdated { get; set; }
}
