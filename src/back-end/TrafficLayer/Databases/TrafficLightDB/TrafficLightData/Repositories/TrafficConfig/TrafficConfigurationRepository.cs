using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.TrafficConfig;

public class TrafficConfigurationRepository : ITrafficConfigurationRepository
{
    private readonly TrafficLightDbContext _db;
    public TrafficConfigurationRepository(TrafficLightDbContext db) => _db = db;

    public Task<TrafficConfiguration?> GetActiveAsync(Guid intersectionId, DateTimeOffset atUtc, CancellationToken ct) =>
        _db.TrafficConfigurations.AsNoTracking()
            .Where(c => c.IntersectionId == intersectionId && c.EffectiveFrom <= atUtc)
            .OrderByDescending(c => c.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

    public Task<TrafficConfiguration?> GetLatestAsync(Guid intersectionId, CancellationToken ct) =>
        _db.TrafficConfigurations.AsNoTracking()
            .Where(c => c.IntersectionId == intersectionId)
            .OrderByDescending(c => c.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

    public Task<List<TrafficConfiguration>> GetHistoryAsync(Guid intersectionId, int take, int skip, CancellationToken ct) =>
        _db.TrafficConfigurations.AsNoTracking()
            .Where(c => c.IntersectionId == intersectionId)
            .OrderByDescending(c => c.EffectiveFrom)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public Task<bool> ChangeRefExistsAsync(string changeRef, CancellationToken ct) =>
        _db.TrafficConfigurations.AsNoTracking()
            .AnyAsync(c => c.ChangeRef == changeRef, ct);

    public async Task<TrafficConfiguration> AddAsync(
        Guid intersectionId,
        JsonDocument pattern,
        DateTimeOffset effectiveFromUtc,
        string? reason,
        string? changeRef,
        string? createdBy,
        CancellationToken ct)
    {
        var row = new TrafficConfiguration
        {
            ConfigId = Guid.NewGuid(),
            IntersectionId = intersectionId,
            Pattern = pattern,
            EffectiveFrom = effectiveFromUtc,
            Reason = reason,
            ChangeRef = changeRef,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.TrafficConfigurations.Add(row);
        await _db.SaveChangesAsync(ct);
        return row;
    }
}
