using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.TrafficConfig;

public class TrafficConfigurationRepository : Repository<TrafficConfiguration>, ITrafficConfigurationRepository
{
    private readonly TrafficLightDbContext _context;

    public TrafficConfigurationRepository(TrafficLightDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TrafficConfiguration>> GetByIntersectionAsync(int intersectionId) =>
        await _context.TrafficConfigurations
            .Where(c => c.IntersectionId == intersectionId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
}
