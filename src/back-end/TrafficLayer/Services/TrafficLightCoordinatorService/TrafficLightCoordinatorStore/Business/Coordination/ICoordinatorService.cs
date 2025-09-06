using System;
using TrafficLightCoordinatorStore.Models.Dtos;

namespace TrafficLightCoordinatorStore.Business.Coordination;

public interface ICoordinatorService
{
    Task<ConfigDto> UpsertConfigAsync(Guid intersectionId, string patternJson, CancellationToken ct);
    Task<ConfigDto?> GetConfigAsync(Guid intersectionId, CancellationToken ct);

    Task<PriorityDto> HandlePriorityAsync(PriorityDto dto, CancellationToken ct);
    Task<CongestionDto> HandleCongestionAsync(CongestionDto dto, CancellationToken ct);
}