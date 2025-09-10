using System;
using System.Text.Json;
using TrafficLightData.Entities;

namespace TrafficLightData.Repositories.TrafficConfig;

public interface ITrafficConfigurationRepository
{
    /// Latest by EffectiveFrom (<= now if you pass now).
    Task<TrafficConfiguration?> GetActiveAsync(Guid intersectionId, DateTimeOffset atUtc, CancellationToken ct);

    /// Absolute latest row regardless of time.
    Task<TrafficConfiguration?> GetLatestAsync(Guid intersectionId, CancellationToken ct);

    /// Paged history (optional convenience).
    Task<List<TrafficConfiguration>> GetHistoryAsync(Guid intersectionId, int take, int skip, CancellationToken ct);

    /// Returns true if a row with this idempotency key already exists.
    Task<bool> ChangeRefExistsAsync(string changeRef, CancellationToken ct);

    /// Inserts a new row (no update-in-place), returns persisted entity.
    Task<TrafficConfiguration> AddAsync(Guid intersectionId, JsonDocument pattern, DateTimeOffset effectiveFromUtc, string? reason, string? changeRef, string? createdBy, CancellationToken ct);
}