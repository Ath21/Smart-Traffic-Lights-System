using System;
using Microsoft.EntityFrameworkCore;
using TrafficDataAnalyticsData;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Congestion;

public class CongestionAlertRepository
{
    private readonly TrafficDataAnalyticsDbContext _context;

    public CongestionAlertRepository(TrafficDataAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CongestionAlert entry)
    {
        await _context.CongestionAlerts.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<CongestionAlert>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date)
    {
        return await _context.CongestionAlerts
            .Where(v => v.IntersectionId == intersectionId && v.Timestamp == date)
            .ToListAsync();
    }
}
