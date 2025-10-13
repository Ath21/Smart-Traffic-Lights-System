using AutoMapper;
using DetectionCacheData.Repositories;
using DetectionData.Collections.Detection;
using DetectionData.Extensions;
using DetectionData.Repositories.EmergencyVehicle;
using DetectionData.Repositories.Incident;
using DetectionData.Repositories.PublicTransport;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;
using DetectionStore.Publishers.Event;
using DetectionStore.Publishers.Logs;
using Messages.Log;
using Messages.Sensor;
using Microsoft.Extensions.Logging;

namespace DetectionStore.Business;

public class DetectionBusiness : IDetectionBusiness
{
    private readonly IMapper _mapper;
    private readonly ILogger<DetectionBusiness> _logger;

    // Data Repositories
    private readonly IEmergencyVehicleDetectionRepository _emergencyRepo;
    private readonly IPublicTransportDetectionRepository _publicRepo;
    private readonly IIncidentDetectionRepository _incidentRepo;
    private readonly IDetectionCacheRepository _cacheRepo;

    // Messaging Publishers
    private readonly IDetectionEventPublisher _eventPublisher;
    private readonly IDetectionLogPublisher _logPublisher;

    public DetectionBusiness(
        IMapper mapper,
        ILogger<DetectionBusiness> logger,
        IEmergencyVehicleDetectionRepository emergencyRepo,
        IPublicTransportDetectionRepository publicRepo,
        IIncidentDetectionRepository incidentRepo,
        IDetectionCacheRepository cacheRepo,
        IDetectionEventPublisher eventPublisher,
        IDetectionLogPublisher logPublisher)
    {
        _mapper = mapper;
        _logger = logger;
        _emergencyRepo = emergencyRepo;
        _publicRepo = publicRepo;
        _incidentRepo = incidentRepo;
        _cacheRepo = cacheRepo;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
    }

    public async Task ProcessDetectionAsync(DetectionEventMessage detectionMsg)
    {
        switch (detectionMsg.EventType?.ToLowerInvariant())
        {
            // ============================================================
            // EMERGENCY VEHICLE
            // ============================================================
            case "emergency vehicle":
                await _emergencyRepo.InsertAsync(new EmergencyVehicleDetectionCollection
                {
                    IntersectionId = detectionMsg.IntersectionId,
                    Intersection = detectionMsg.IntersectionName,
                    Direction = detectionMsg.Direction,
                    EmergencyVehicleType = detectionMsg.VehicleType,
                    DetectedAt = detectionMsg.Timestamp,
                    Metadata = BsonExtensions.ToBsonDocument(detectionMsg.Metadata)
                });

                await _cacheRepo.SetEmergencyDetectedAsync(detectionMsg.IntersectionId, true);
                _logger.LogInformation("[BUSINESS] Emergency event persisted at {Intersection}", detectionMsg.IntersectionName);
                break;

            // ============================================================
            // PUBLIC TRANSPORT
            // ============================================================
            case "public transport":
                await _publicRepo.InsertAsync(new PublicTransportDetectionCollection
                {
                    IntersectionId = detectionMsg.IntersectionId,
                    IntersectionName = detectionMsg.IntersectionName,
                    LineName = detectionMsg.VehicleType, 
                    DetectedAt = detectionMsg.Timestamp,
                    Metadata = BsonExtensions.ToBsonDocument(detectionMsg.Metadata)
                });

                await _cacheRepo.SetPublicTransportDetectedAsync(detectionMsg.IntersectionId, true);
                _logger.LogInformation("[BUSINESS] Public transport event persisted at {Intersection}", detectionMsg.IntersectionName);
                break;

            // ============================================================
            // INCIDENT
            // ============================================================
            case "incident":
                await _incidentRepo.InsertAsync(new IncidentDetectionCollection
                {
                    IntersectionId = detectionMsg.IntersectionId,
                    Intersection = detectionMsg.IntersectionName,
                    Description = "Incident " + (detectionMsg.Metadata?["incident_type"] ?? "unknown") + " reported",
                    ReportedAt = detectionMsg.Timestamp,    
                    Metadata = BsonExtensions.ToBsonDocument(detectionMsg.Metadata)
                });

                await _cacheRepo.SetIncidentDetectedAsync(detectionMsg.IntersectionId, true);
                _logger.LogWarning("[BUSINESS] Incident event persisted at {Intersection}", detectionMsg.IntersectionName);
                break;

            default:
                _logger.LogWarning("[BUSINESS] Unknown detection type: {Type}", detectionMsg.EventType);
                break;
        }

    }

    public async Task<IEnumerable<EmergencyVehicleDetectionResponse>> GetRecentEmergenciesAsync(int intersectionId)
    {
        var data = await _emergencyRepo.GetRecentEmergenciesAsync(intersectionId);
        return _mapper.Map<IEnumerable<EmergencyVehicleDetectionResponse>>(data);
    }

    public async Task<IEnumerable<PublicTransportDetectionResponse>> GetPublicTransportsAsync(int intersectionId)
    {
        var data = await _publicRepo.GetByLineAsync(""); // optional filter later
        return _mapper.Map<IEnumerable<PublicTransportDetectionResponse>>(data);
    }

    public async Task<IEnumerable<IncidentDetectionResponse>> GetRecentIncidentsAsync(int intersectionId)
    {
        var data = await _incidentRepo.GetRecentIncidentsAsync(intersectionId);
        return _mapper.Map<IEnumerable<IncidentDetectionResponse>>(data);
    }
}
