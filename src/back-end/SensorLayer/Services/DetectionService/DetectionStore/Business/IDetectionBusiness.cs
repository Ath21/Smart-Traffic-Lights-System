using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;

namespace DetectionStore.Business;

public interface IDetectionBusiness
{
    // Emergency
    Task<EmergencyVehicleDetectionResponse> CreateEmergencyAsync(EmergencyVehicleDetectionRequest request);
    Task<IEnumerable<EmergencyVehicleDetectionResponse>> GetRecentEmergenciesAsync(int intersectionId);

    // Public Transport
    Task<PublicTransportDetectionResponse> CreatePublicTransportAsync(PublicTransportDetectionRequest request);
    Task<IEnumerable<PublicTransportDetectionResponse>> GetPublicTransportsAsync(int intersectionId);

    // Incident
    Task<IncidentDetectionResponse> CreateIncidentAsync(IncidentDetectionRequest request);
    Task<IEnumerable<IncidentDetectionResponse>> GetRecentIncidentsAsync(int intersectionId);

    // Cache
    Task<object> GetDetectionFlagsAsync(int intersectionId);
}
