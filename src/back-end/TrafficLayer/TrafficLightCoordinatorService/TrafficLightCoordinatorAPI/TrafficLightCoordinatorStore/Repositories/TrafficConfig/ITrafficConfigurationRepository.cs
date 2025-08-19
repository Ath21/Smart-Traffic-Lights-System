using System;
using TrafficLightCoordinatorData.Entities;

namespace TrafficLightCoordinatorStore.Repositories.TrafficConfig;

public interface ITrafficConfigurationRepository
{
    Task<TrafficConfiguration?> GetLatestAsync(Guid intersectionId, CancellationToken ct);
    Task<TrafficConfiguration> UpsertAsync(Guid intersectionId, string patternJson, CancellationToken ct);
}
