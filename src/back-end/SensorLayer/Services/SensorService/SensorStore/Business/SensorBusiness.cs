using AutoMapper;
using DetectionCacheData.Repositories;
using DetectionData.Collections.Count;
using DetectionData.Repositories.Vehicle;
using DetectionData.Repositories.Pedestrian;
using DetectionData.Repositories.Cyclist;
using SensorStore.Models.Requests;
using SensorStore.Models.Responses;

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
    // VEHICLE COUNTS
    // ============================================================

    public async Task<VehicleCountResponse> RecordVehicleCountAsync(VehicleCountRequest request)
    {
        var entity = _mapper.Map<VehicleCountCollection>(request);
        entity.Timestamp = DateTime.UtcNow;

        await _vehicleRepo.InsertAsync(entity);
        await _cacheRepo.SetVehicleCountAsync(request.IntersectionId, request.CountTotal);

        _logger.LogInformation(
            "[BUSINESS] Vehicle count stored for {Intersection} (Total={Total}, AvgSpeed={Speed} km/h)",
            request.Intersection, request.CountTotal, request.AverageSpeedKmh);

        return _mapper.Map<VehicleCountResponse>(entity);
    }

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

    public async Task<PedestrianCountResponse> RecordPedestrianCountAsync(PedestrianCountRequest request)
    {
        var entity = _mapper.Map<PedestrianCountCollection>(request);
        entity.Timestamp = DateTime.UtcNow;

        await _pedestrianRepo.InsertAsync(entity);
        await _cacheRepo.SetPedestrianCountAsync(request.IntersectionId, request.Count);

        _logger.LogInformation(
            "[BUSINESS] Pedestrian count stored for {Intersection} (Count={Count})",
            request.Intersection, request.Count);

        return _mapper.Map<PedestrianCountResponse>(entity);
    }

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

    public async Task<CyclistCountResponse> RecordCyclistCountAsync(CyclistCountRequest request)
    {
        var entity = _mapper.Map<CyclistCountCollection>(request);
        entity.Timestamp = DateTime.UtcNow;

        await _cyclistRepo.InsertAsync(entity);
        await _cacheRepo.SetCyclistCountAsync(request.IntersectionId, request.Count);

        _logger.LogInformation(
            "[BUSINESS] Cyclist count stored for {Intersection} (Count={Count})",
            request.Intersection, request.Count);

        return _mapper.Map<CyclistCountResponse>(entity);
    }

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
