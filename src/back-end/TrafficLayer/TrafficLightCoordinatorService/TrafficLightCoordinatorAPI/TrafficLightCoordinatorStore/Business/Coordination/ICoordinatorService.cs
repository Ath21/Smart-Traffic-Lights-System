using System;

namespace TrafficLightCoordinatorStore.Business.Coordination;

public interface ICoordinatorService
{
    Task<ConfigResponseDto> UpsertConfigAsync(Guid intersectionId, string patternJson, CancellationToken ct);
    Task<ConfigResponseDto?> GetConfigAsync(Guid intersectionId, CancellationToken ct);

    Task HandlePriorityAsync(PriorityMessage message, CancellationToken ct);
    Task HandleCongestionAsync(TrafficCongestionAlert alert, CancellationToken ct);
}