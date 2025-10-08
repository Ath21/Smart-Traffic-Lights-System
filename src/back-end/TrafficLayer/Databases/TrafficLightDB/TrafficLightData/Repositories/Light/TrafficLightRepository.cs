using System;
using Microsoft.EntityFrameworkCore;

using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.Light;

public class TrafficLightRepository : BaseRepository<TrafficLightEntity>, ITrafficLightRepository
{
    public TrafficLightRepository(TrafficLightDbContext context) : base(context) { }

    public async Task<IEnumerable<TrafficLightEntity>> GetByIntersectionAsync(int intersectionId)
        => await _context.TrafficLights
            .Where(l => l.IntersectionId == intersectionId)
            .OrderBy(l => l.LightName)
            .ToListAsync();

    public async Task UpdateStatusAsync(int lightId, bool isOperational)
    {
        var light = await _context.TrafficLights.FindAsync(lightId);
        if (light != null)
        {
            light.IsOperational = isOperational;
            await _context.SaveChangesAsync();
        }
    }
}