using System;
using TrafficLightCoordinatorStore.Models;

namespace TrafficLightCoordinatorStore.Business.Coordination;

public interface IScheduleService
{
    Task<GetScheduleResponseDto?> GetScheduleAsync(Guid intersectionId, CancellationToken ct);
    Task<GetScheduleResponseDto> UpdateScheduleAsync(Guid intersectionId, List<PhaseDto> phases, CancellationToken ct);
    Task<GetScheduleResponseDto?> ApplyPriorityAsync(Guid intersectionId, string priorityType, CancellationToken ct);
}
