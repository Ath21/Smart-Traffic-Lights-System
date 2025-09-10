namespace DetectionStore.Business;


public interface ISensorDetectionService
{
    Task<DetectionSnapshotDto?> GetSnapshotAsync(Guid intersectionId);
    Task<IEnumerable<DetectionHistoryDto>> GetHistoryAsync(Guid intersectionId);
    Task<EmergencyVehicleDto> RecordEmergencyAsync(EmergencyVehicleDto dto);
    Task<PublicTransportDto> RecordPublicTransportAsync(PublicTransportDto dto);
    Task<IncidentDto> RecordIncidentAsync(IncidentDto dto);
}