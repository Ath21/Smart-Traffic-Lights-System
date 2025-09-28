using System;
using DetectionStore.Models.Dtos;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;

namespace DetectionStore.Business;

public interface IDetectionEventService
{
    Task<IEnumerable<DetectionEventResponse>> GetActiveEventsAsync(int intersectionId);
    Task<DetectionEventResponse> ReportEventAsync(DetectionEventRequest request);
    Task RecordEmergencyAsync(EmergencyVehicleDto dto);
    Task RecordPublicTransportAsync(PublicTransportDto dto);
    Task RecordIncidentAsync(IncidentDto dto);

}
