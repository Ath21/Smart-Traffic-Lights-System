using System;
using Microsoft.EntityFrameworkCore;
using TrafficDataAnalyticsData;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Summary;

public class DailySummaryRepository : IDailySummaryRepository
{
        private readonly TrafficDataAnalyticsDbContext _context;

    public DailySummaryRepository(TrafficDataAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DailySummary entry)
    {
        await _context.DailySummaries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<DailySummary>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date)
    {
        return await _context.DailySummaries
            .Where(v => v.IntersectionId == intersectionId && v.Date == date)
            .ToListAsync();
    }

    public Task<List<DailySummary>> GetRangeByIntersectionAsync(string intersectionId, DateTime from, DateTime to)
    {
    return _context.DailySummaries
            .Where(v => v.IntersectionId == intersectionId && v.Date >= from && v.Date <= to)
            .ToListAsync();
    }
}
