using System;
using Microsoft.EntityFrameworkCore;
using TrafficDataAnalyticsData;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Cyclist;

public class CyclistCountRepository : ICyclistCountRepository
{
    private readonly TrafficDataAnalyticsDbContext _context;

    public CyclistCountRepository(TrafficDataAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CyclistCount entry)
    {
        await _context.CyclistCounts.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<CyclistCount>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date)
    {
        return await _context.CyclistCounts
            .Where(v => v.IntersectionId == intersectionId && v.Timestamp == date)
            .ToListAsync();
    }
}
