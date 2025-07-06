using System;
using Microsoft.EntityFrameworkCore;
using TrafficDataAnalyticsData;
using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Vehicle;

public class VehicleCountRepository : IVehicleCountRepository
{
    private readonly TrafficDataAnalyticsDbContext _context;

    public VehicleCountRepository(TrafficDataAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(VehicleCount entry)
    {
        await _context.VehicleCounts.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<VehicleCount>> GetByIntersectionAndDateAsync(string intersectionId, DateTime date)
    {
        return await _context.VehicleCounts
            .Where(v => v.IntersectionId == intersectionId && v.Timestamp == date)
            .ToListAsync();
    }
}
