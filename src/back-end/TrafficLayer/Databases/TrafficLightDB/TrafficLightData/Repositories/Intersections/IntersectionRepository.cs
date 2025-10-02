using System;
using Microsoft.EntityFrameworkCore;
using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.Intersections;


public class IntersectionRepository : Repository<Intersection>, IIntersectionRepository
{
    private readonly TrafficLightDbContext _context;

    public IntersectionRepository(TrafficLightDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Intersection?> GetWithDetailsAsync(int id) =>
        await _context.Intersections
            .Include(i => i.TrafficLights)
            .Include(i => i.Configurations)
            .FirstOrDefaultAsync(i => i.IntersectionId == id);
}