using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.TrafficConfig;

public class TrafficConfigurationRepository : BaseRepository<TrafficConfigurationEntity>, ITrafficConfigurationRepository
{
    public TrafficConfigurationRepository(TrafficLightDbContext context) : base(context) { }

    public async Task<TrafficConfigurationEntity?> GetLatestByIntersectionAsync(int configId)
        => await _context.TrafficConfigurations
            .Where(c => c.ConfigurationId == configId)
            .OrderByDescending(c => c.LastUpdated)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<TrafficConfigurationEntity>> GetByModeAsync(string mode)
        => await _context.TrafficConfigurations
            .Where(c => c.Mode == mode)
            .OrderByDescending(c => c.LastUpdated)
            .ToListAsync();
}
