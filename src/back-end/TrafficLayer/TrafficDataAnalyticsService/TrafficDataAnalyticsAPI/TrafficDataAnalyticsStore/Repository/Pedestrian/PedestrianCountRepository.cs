using System;
using Microsoft.EntityFrameworkCore;
using TrafficDataAnalyticsData;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Pedestrian;

public class PedestrianCountRepository : IPedestrianCountRepository
{
    private readonly TrafficDataAnalyticsDbContext _context;

    public PedestrianCountRepository(TrafficDataAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PedestrianCount entry)
    {
        await _context.PedestrianCounts.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PedestrianCount>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date)
    {
        return await _context.PedestrianCounts
            .Where(v => v.IntersectionId == intersectionId && v.Timestamp == date)
            .ToListAsync();
    }
}
