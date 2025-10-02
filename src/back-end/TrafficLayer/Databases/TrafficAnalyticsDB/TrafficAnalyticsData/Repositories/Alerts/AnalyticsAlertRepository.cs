using Microsoft.EntityFrameworkCore;
using TrafficAnalyticsData;
using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsData.Repositories.Alerts;

public class AnalyticsAlertRepository : Repository<Alert>, IAnalyticsAlertRepository
{
    private readonly TrafficAnalyticsDbContext _context;

    public AnalyticsAlertRepository(TrafficAnalyticsDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Alert>> GetByIntersectionAsync(int intersectionId) =>
        await _context.Alerts
            .Where(a => a.IntersectionId == intersectionId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Alert>> GetRecentAlertsAsync(int count = 10) =>
        await _context.Alerts
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
}