using System;
using Microsoft.EntityFrameworkCore;

using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.Light;

public class TrafficLightRepository : Repository<TrafficLight>, ITrafficLightRepository
{
    private readonly TrafficLightDbContext _context;

    public TrafficLightRepository(TrafficLightDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TrafficLight>> GetByIntersectionAsync(int intersectionId) =>
        await _context.TrafficLights
            .Where(l => l.IntersectionId == intersectionId)
            .ToListAsync();

    public async Task<IEnumerable<TrafficLight>> GetByStateAsync(TrafficLightState state) =>
        await _context.TrafficLights
            .Where(l => l.CurrentState == state)
            .ToListAsync();
}
