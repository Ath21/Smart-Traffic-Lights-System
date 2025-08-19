using System;
using System.Text.Json;
using AutoMapper;
using TrafficLightCoordinatorStore.Models;
using TrafficLightCoordinatorStore.Repositories.Intersections;
using TrafficLightCoordinatorStore.Repositories.TrafficConfig;

namespace TrafficLightCoordinatorStore.Business.Coordination;

public class ScheduleService : IScheduleService
{
    private readonly ITrafficConfigurationRepository _configRepo;
    private readonly IIntersectionRepository _intRepo;
    private readonly ILightUpdatePublisher _publisher;
    private readonly IMapper _mapper;

    public ScheduleService(
        ITrafficConfigurationRepository configRepo,
        IIntersectionRepository intRepo,
        ILightUpdatePublisher publisher,
        IMapper mapper)
    {
        _configRepo = configRepo;
        _intRepo = intRepo;
        _publisher = publisher;
        _mapper = mapper;
    }

    public async Task<GetScheduleResponseDto?> GetScheduleAsync(Guid intersectionId, CancellationToken ct)
    {
        var cfg = await _configRepo.GetLatestAsync(intersectionId, ct);
        return cfg is null ? null : _mapper.Map<GetScheduleResponseDto>(cfg);
    }

    public async Task<GetScheduleResponseDto> UpdateScheduleAsync(Guid intersectionId, List<PhaseDto> phases, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(new
        {
            phases = phases.Select(p => new { phase = p.Phase, duration = p.Duration }),
            updated_at = DateTimeOffset.UtcNow
        });

        var saved = await _configRepo.UpsertAsync(intersectionId, json, ct);
        var dto = _mapper.Map<GetScheduleResponseDto>(saved);

        // Publish coordinated update
        await _publisher.PublishAsync(intersectionId, dto.Schedule_Pattern, ct);

        return dto;
    }

    public async Task<GetScheduleResponseDto?> ApplyPriorityAsync(Guid intersectionId, string priorityType, CancellationToken ct)
    {
        // get base schedule or default
        var current = await GetScheduleAsync(intersectionId, ct);
        if (current is null)
        {
            // default fallback pattern
            current = new GetScheduleResponseDto(
                intersectionId,
                new SchedulePatternDto(
                    new() { new PhaseDto("NS_Green", 30), new PhaseDto("EW_Green", 30) },
                    DateTimeOffset.UtcNow));
        }

        var recalculated = CoordinationEngine.Recalculate(current.Schedule_Pattern.Phases, priorityType);
        var result = await UpdateScheduleAsync(intersectionId, recalculated, ct);
        return result;
    }
}