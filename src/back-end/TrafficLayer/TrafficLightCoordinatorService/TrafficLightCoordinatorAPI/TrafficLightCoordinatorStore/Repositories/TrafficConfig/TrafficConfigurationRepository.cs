using System;
using Microsoft.EntityFrameworkCore;
using TrafficLightCoordinatorData;
using TrafficLightCoordinatorData.Entities;

namespace TrafficLightCoordinatorStore.Repositories.TrafficConfig;

public class TrafficConfigurationRepository : ITrafficConfigurationRepository
{
    private readonly TrafficLightCoordinatorDbContext _db;
    public TrafficConfigurationRepository(TrafficLightCoordinatorDbContext db) => _db = db;

    public Task<TrafficConfiguration?> GetLatestAsync(Guid intersectionId, CancellationToken ct) =>
        _db.TrafficConfigurations
            .AsNoTracking()
            .Where(c => c.IntersectionId == intersectionId)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<TrafficConfiguration> UpsertAsync(Guid intersectionId, string patternJson, CancellationToken ct)
    {
        var existing = await _db.TrafficConfigurations
            .Where(c => c.IntersectionId == intersectionId)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        if (existing is null)
        {
            existing = new TrafficConfiguration
            {
                ConfigId = Guid.NewGuid(),
                IntersectionId = intersectionId,
                Pattern = patternJson,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.TrafficConfigurations.Add(existing);
        }
        else
        {
            existing.Pattern = patternJson;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            _db.TrafficConfigurations.Update(existing);
        }

        await _db.SaveChangesAsync(ct);
        return existing;
    }
}
