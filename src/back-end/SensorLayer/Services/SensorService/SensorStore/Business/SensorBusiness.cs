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

    // ============================================================
    // PROCESS SENSOR COUNT MESSAGE (from worker or event)
    // ============================================================
    public async Task ProcessSensorAsync(SensorCountMessage sensorMsg)
    {
        switch (sensorMsg.CountType?.ToLowerInvariant())
        {
            // ============================================================
            // VEHICLE COUNT
            // ============================================================
            case "vehicle":
                var vehicleDoc = new VehicleCountCollection
                {
                    IntersectionId = sensorMsg.IntersectionId,
                    Intersection = sensorMsg.IntersectionName,
                    Timestamp = sensorMsg.Timestamp,
                    CountTotal = sensorMsg.Count,
                    AverageSpeedKmh = sensorMsg.AverageSpeedKmh,
                    AverageWaitTimeSec = sensorMsg.AverageWaitTimeSec,
                    FlowRate = sensorMsg.FlowRate,
                    VehicleBreakdown = sensorMsg.Breakdown
                };

                await _vehicleRepo.InsertAsync(vehicleDoc);
                await _cacheRepo.SetVehicleCountAsync(sensorMsg.IntersectionId, sensorMsg.Count);

                _logger.LogInformation(
                    "[BUSINESS] Vehicle count persisted at {Intersection} | Total={Total}, FlowRate={FlowRate}",
                    sensorMsg.IntersectionName, sensorMsg.Count, sensorMsg.FlowRate);
                break;

            // ============================================================
            // PEDESTRIAN COUNT
            // ============================================================
            case "pedestrian":
                var pedDoc = new PedestrianCountCollection
                {
                    IntersectionId = sensorMsg.IntersectionId,
                    Intersection = sensorMsg.IntersectionName,
                    Timestamp = sensorMsg.Timestamp,
                    Count = sensorMsg.Count
                };

                await _pedestrianRepo.InsertAsync(pedDoc);
                await _cacheRepo.SetPedestrianCountAsync(sensorMsg.IntersectionId, sensorMsg.Count);

                _logger.LogInformation(
                    "[BUSINESS] Pedestrian count persisted at {Intersection} | Count={Count}",
                    sensorMsg.IntersectionName, sensorMsg.Count);
                break;

            // ============================================================
            // CYCLIST COUNT
            // ============================================================
            case "cyclist":
                var cycDoc = new CyclistCountCollection
                {
                    IntersectionId = sensorMsg.IntersectionId,
                    Intersection = sensorMsg.IntersectionName,
                    Timestamp = sensorMsg.Timestamp,
                    Count = sensorMsg.Count
                };

                await _cyclistRepo.InsertAsync(cycDoc);
                await _cacheRepo.SetCyclistCountAsync(sensorMsg.IntersectionId, sensorMsg.Count);

                _logger.LogInformation(
                    "[BUSINESS] Cyclist count persisted at {Intersection} | Count={Count}",
                    sensorMsg.IntersectionName, sensorMsg.Count);
                break;

            default:
                _logger.LogWarning(
                    "[BUSINESS] Unknown sensor count type: {Type} (Intersection={Intersection})",
                    sensorMsg.CountType, sensorMsg.IntersectionName);
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
