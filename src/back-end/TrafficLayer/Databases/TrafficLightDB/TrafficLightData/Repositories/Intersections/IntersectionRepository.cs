using System;
using Microsoft.EntityFrameworkCore;
using TrafficLightData.Entities;


namespace TrafficLightData.Repositories.Intersections;

public class IntersectionRepository : BaseRepository<IntersectionEntity>, IIntersectionRepository
{
    public IntersectionRepository(TrafficLightDbContext context) : base(context) { }

    public async Task<IEnumerable<IntersectionEntity>> GetAllActiveAsync()
        => await _context.Intersections
            .Include(i => i.TrafficLights)
            .Include(i => i.Configurations)
            .Where(i => i.IsActive)
            .OrderBy(i => i.Name)
            .ToListAsync();

    public async Task<IntersectionEntity?> GetByNameAsync(string name)
        => await _context.Intersections
            .Include(i => i.TrafficLights)
            .Include(i => i.Configurations)
            .FirstOrDefaultAsync(i => i.Name == name);
}