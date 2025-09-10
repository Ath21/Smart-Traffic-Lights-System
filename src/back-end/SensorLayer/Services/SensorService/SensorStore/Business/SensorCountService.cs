using AutoMapper;
using DetectionCacheData.Entities;
using DetectionCacheData.Repositories.Cache;
using SensorStore.Models.Dtos;
using SensorStore.Publishers.Count;
using SensorStore.Publishers.Logs;

namespace SensorStore.Business;

public class SensorCountService : ISensorCountService
{
    private readonly ISensorCacheRepository _cacheRepo;
    private readonly ISensorCountPublisher _countPublisher;
    private readonly ISensorLogPublisher _logPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<SensorCountService> _logger;

    public SensorCountService(
        ISensorCacheRepository cacheRepo,
        ISensorCountPublisher countPublisher,
        ISensorLogPublisher logPublisher,
        IMapper mapper,
        ILogger<SensorCountService> logger)
    {
        _cacheRepo = cacheRepo;
        _countPublisher = countPublisher;
        _logPublisher = logPublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SensorSnapshotDto?> GetSnapshotAsync(Guid intersectionId)
    {
        var entity = await _cacheRepo.GetSnapshotAsync(intersectionId);
        return entity == null ? null : _mapper.Map<SensorSnapshotDto>(entity);
    }

    public async Task<IEnumerable<SensorHistoryDto>> GetHistoryAsync(Guid intersectionId)
    {
        // Redis snapshot-only; return mock/stub until Mongo history is added
        return new List<SensorHistoryDto>
        {
            new() { Timestamp = DateTime.UtcNow.AddMinutes(-10), VehicleCount = 15, PedestrianCount = 6, CyclistCount = 2 },
            new() { Timestamp = DateTime.UtcNow, VehicleCount = 20, PedestrianCount = 7, CyclistCount = 3 }
        };
    }

    public async Task<SensorSnapshotDto> UpdateSnapshotAsync(SensorSnapshotDto dto, float avgSpeed)
    {
        var entity = _mapper.Map<DetectionCache>(dto);
        await _cacheRepo.SetSnapshotAsync(dto.IntersectionId, entity);

        // Publish events
        await _countPublisher.PublishVehicleCountAsync(dto.IntersectionId, dto.VehicleCount, avgSpeed);
        await _countPublisher.PublishPedestrianCountAsync(dto.IntersectionId, dto.PedestrianCount);
        await _countPublisher.PublishCyclistCountAsync(dto.IntersectionId, dto.CyclistCount);

        await _logPublisher.PublishAuditAsync("UpdateSnapshot", $"Updated snapshot for {dto.IntersectionId}", dto);

        return dto;
    }
}
