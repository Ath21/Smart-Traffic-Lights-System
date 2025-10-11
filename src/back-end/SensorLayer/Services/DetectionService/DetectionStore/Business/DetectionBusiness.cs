using AutoMapper;
using DetectionCacheData.Repositories;
using DetectionData.Collections.Detection;
using DetectionData.Repositories.EmergencyVehicle;
using DetectionData.Repositories.Incident;
using DetectionData.Repositories.PublicTransport;
using DetectionStore.Models.Requests;
using DetectionStore.Models.Responses;
using DetectionStore.Publishers.Event;
using DetectionStore.Publishers.Logs;
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

    // ============================================================
    // EMERGENCY VEHICLE DETECTION
    // ============================================================
    public async Task<EmergencyVehicleDetectionResponse> CreateEmergencyAsync(EmergencyVehicleDetectionRequest request)
    {
        try
        {
            var entity = _mapper.Map<EmergencyVehicleDetectionCollection>(request);
            await _emergencyRepo.InsertAsync(entity);

            await _cacheRepo.SetEmergencyDetectedAsync(request.IntersectionId, true);

            // Publish detection event
            await _eventPublisher.PublishEmergencyVehicleAsync(
                vehicleType: request.EmergencyVehicleType,
                speed_kmh: 0,
                direction: request.Direction);

            // Publish audit log
            await _logPublisher.PublishAuditAsync(
                action: "EmergencyVehicleDetected",
                message: $"Emergency vehicle ({request.EmergencyVehicleType}) detected from {request.Direction} at {request.Intersection}");

            _logger.LogInformation("[Detection] Emergency vehicle detected at {Intersection}", request.Intersection);
            return _mapper.Map<EmergencyVehicleDetectionResponse>(entity);
        }
        catch (Exception ex)
        {
            await _logPublisher.PublishErrorAsync(
                action: "CreateEmergencyAsync",
                errorMessage: ex.Message,
                ex: ex);

            _logger.LogError(ex, "[Detection] Failed to create emergency detection");
            throw;
        }
    }

    public async Task<IEnumerable<EmergencyVehicleDetectionResponse>> GetRecentEmergenciesAsync(int intersectionId)
    {
        var data = await _emergencyRepo.GetRecentEmergenciesAsync(intersectionId);
        return _mapper.Map<IEnumerable<EmergencyVehicleDetectionResponse>>(data);
    }

    // ============================================================
    // PUBLIC TRANSPORT DETECTION
    // ============================================================
    public async Task<PublicTransportDetectionResponse> CreatePublicTransportAsync(PublicTransportDetectionRequest request)
    {
        try
        {
            var entity = _mapper.Map<PublicTransportDetectionCollection>(request);
            await _publicRepo.InsertAsync(entity);

            await _cacheRepo.SetPublicTransportDetectedAsync(request.IntersectionId, true);

            // Publish detection event
            await _eventPublisher.PublishPublicTransportAsync(
                mode: "Bus",
                line: request.LineName,
                arrival_estimated_sec: 0,
                direction: "unknown");

            // Publish audit log
            await _logPublisher.PublishAuditAsync(
                action: "PublicTransportDetected",
                message: $"Public transport ({request.LineName}) detected at {request.IntersectionName}");

            _logger.LogInformation("[Detection] Public transport ({Line}) detected at {Intersection}",
                request.LineName, request.IntersectionName);

            return _mapper.Map<PublicTransportDetectionResponse>(entity);
        }
        catch (Exception ex)
        {
            await _logPublisher.PublishErrorAsync(
                action: "CreatePublicTransportAsync",
                errorMessage: ex.Message,
                ex: ex);

            _logger.LogError(ex, "[Detection] Failed to create public transport detection");
            throw;
        }
    }

    public async Task<IEnumerable<PublicTransportDetectionResponse>> GetPublicTransportsAsync(int intersectionId)
    {
        var data = await _publicRepo.GetByLineAsync(""); // optional filter later
        return _mapper.Map<IEnumerable<PublicTransportDetectionResponse>>(data);
    }

    // ============================================================
    // INCIDENT DETECTION
    // ============================================================
    public async Task<IncidentDetectionResponse> CreateIncidentAsync(IncidentDetectionRequest request)
    {
        try
        {
            var entity = _mapper.Map<IncidentDetectionCollection>(request);
            await _incidentRepo.InsertAsync(entity);

            await _cacheRepo.SetIncidentDetectedAsync(request.IntersectionId, true);

            // Publish detection event
            await _eventPublisher.PublishIncidentAsync(
                type: "Incident",
                severity: 2,
                description: request.Description,
                direction: "unknown");

            // Publish audit log
            await _logPublisher.PublishAuditAsync(
                action: "IncidentDetected",
                message: $"Incident detected at {request.Intersection}: {request.Description}");

            _logger.LogWarning("[Detection] Incident detected at {Intersection}: {Desc}",
                request.Intersection, request.Description);

            return _mapper.Map<IncidentDetectionResponse>(entity);
        }
        catch (Exception ex)
        {
            await _logPublisher.PublishErrorAsync(
                action: "CreateIncidentAsync",
                errorMessage: ex.Message,
                ex: ex);

            _logger.LogError(ex, "[Detection] Failed to create incident detection");
            throw;
        }
    }

    public async Task<IEnumerable<IncidentDetectionResponse>> GetRecentIncidentsAsync(int intersectionId)
    {
        var data = await _incidentRepo.GetRecentIncidentsAsync(intersectionId);
        return _mapper.Map<IEnumerable<IncidentDetectionResponse>>(data);
    }

    // ============================================================
    // CACHE FLAGS (for local intersection controllers)
    // ============================================================
    public async Task<object> GetDetectionFlagsAsync(int intersectionId)
    {
        var emergency = await _cacheRepo.GetEmergencyDetectedAsync(intersectionId);
        var incident = await _cacheRepo.GetIncidentDetectedAsync(intersectionId);
        var transport = await _cacheRepo.GetPublicTransportDetectedAsync(intersectionId);

        return new
        {
            intersectionId,
            emergency,
            incident,
            publicTransport = transport
        };
    }
}
