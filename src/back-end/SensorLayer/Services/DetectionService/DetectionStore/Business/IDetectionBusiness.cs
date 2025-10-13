using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;
using MassTransit.Logging;
using Messages.Log;
using Messages.Sensor;

namespace DetectionStore.Business;

public interface IDetectionBusiness
{
    Task<IEnumerable<EmergencyVehicleDetectionResponse>> GetRecentEmergenciesAsync(int intersectionId);
    Task<IEnumerable<PublicTransportDetectionResponse>> GetPublicTransportsAsync(int intersectionId);
    Task<IEnumerable<IncidentDetectionResponse>> GetRecentIncidentsAsync(int intersectionId);
    Task ProcessDetectionAsync(DetectionEventMessage detectionMsg);
}
