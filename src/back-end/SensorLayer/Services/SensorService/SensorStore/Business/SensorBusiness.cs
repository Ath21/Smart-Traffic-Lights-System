using AutoMapper;
using DetectionCacheData.Repositories;
using DetectionData.Collections.Count;
using DetectionData.Repositories.Vehicle;
using DetectionData.Repositories.Pedestrian;
using DetectionData.Repositories.Cyclist;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;
using Messages.Sensor;

namespace SensorStore.Business;

// ============================================================
// Business Layer: Sensor Service
// ------------------------------------------------------------
// Handles traffic count persistence (vehicles, pedestrians,
// cyclists) to MongoDB and cache synchronization to Redis.
// ------------------------------------------------------------
// Updated by : Sensor Service
// Consumed by : Sensor API, Sensor Worker
// ============================================================

public class SensorBusiness : ISensorBusiness
{
    private readonly IVehicleCountRepository _vehicleRepo;
    private readonly IPedestrianCountRepository _pedestrianRepo;
    private readonly ICyclistCountRepository _cyclistRepo;
    private readonly IDetectionCacheRepository _cacheRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<SensorBusiness> _logger;

    public SensorBusiness(
        IVehicleCountRepository vehicleRepo,
        IPedestrianCountRepository pedestrianRepo,
        ICyclistCountRepository cyclistRepo,
        IDetectionCacheRepository cacheRepo,
        IMapper mapper,
        ILogger<SensorBusiness> logger)
    {
        _vehicleRepo = vehicleRepo;
        _pedestrianRepo = pedestrianRepo;
        _cyclistRepo = cyclistRepo;
        _cacheRepo = cacheRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task ProcessSensorAsync(SensorCountMessage sensorMsg)
    {
        switch (sensorMsg.CountType?.ToLowerInvariant())
        {
            // ============================================================
            // VEHICLE
            // ============================================================
            case "vehicle":
                await _vehicleRepo.InsertAsync(new VehicleCountCollection
                {
                    IntersectionId = sensorMsg.IntersectionId,
                    Intersection = sensorMsg.IntersectionName,
                    Direction = sensorMsg,
                    EmergencyVehicleType = detectionMsg.VehicleType,
                    DetectedAt = detectionMsg.Timestamp
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
                    LineName = detectionMsg.VehicleType, // using VehicleType as line name
                    DetectedAt = detectionMsg.Timestamp
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
                    Description = detectionMsg.Metadata?["description"] ?? "unknown",
                    ReportedAt = detectionMsg.Timestamp
                });

                await _cacheRepo.SetIncidentDetectedAsync(detectionMsg.IntersectionId, true);
                _logger.LogWarning("[BUSINESS] Incident event persisted at {Intersection}", detectionMsg.IntersectionName);
                break;

            default:
                _logger.LogWarning("[BUSINESS] Unknown detection type: {Type}", detectionMsg.EventType);
                break;
        }

    }

    // ============================================================
    // VEHICLE COUNTS
    // ============================================================
    public async Task<IEnumerable<VehicleCountResponse>> GetRecentVehicleCountsAsync(int intersectionId)
    {
        var data = await _vehicleRepo.GetRecentByIntersectionAsync(intersectionId);
        var mapped = _mapper.Map<IEnumerable<VehicleCountResponse>>(data);

        _logger.LogInformation(
            "[BUSINESS] Retrieved {Count} vehicle count records for intersection {Id}",
            mapped.Count(), intersectionId);

        return mapped;
    }

    // ============================================================
    // PEDESTRIAN COUNTS
    // ============================================================
    public async Task<IEnumerable<PedestrianCountResponse>> GetRecentPedestrianCountsAsync(int intersectionId)
    {
        var data = await _pedestrianRepo.GetRecentByIntersectionAsync(intersectionId);
        var mapped = _mapper.Map<IEnumerable<PedestrianCountResponse>>(data);

        _logger.LogInformation(
            "[BUSINESS] Retrieved {Count} pedestrian count records for intersection {Id}",
            mapped.Count(), intersectionId);

        return mapped;
    }

    // ============================================================
    // CYCLIST COUNTS
    // ============================================================
    public async Task<IEnumerable<CyclistCountResponse>> GetRecentCyclistCountsAsync(int intersectionId)
    {
        var data = await _cyclistRepo.GetRecentByIntersectionAsync(intersectionId);
        var mapped = _mapper.Map<IEnumerable<CyclistCountResponse>>(data);

        _logger.LogInformation(
            "[BUSINESS] Retrieved {Count} cyclist count records for intersection {Id}",
            mapped.Count(), intersectionId);

        return mapped;
    }
}
