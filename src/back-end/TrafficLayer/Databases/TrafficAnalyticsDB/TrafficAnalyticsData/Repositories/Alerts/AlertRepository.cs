using Microsoft.EntityFrameworkCore;
using TrafficAnalyticsData;
using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsData.Repositories.Alerts;

public class AlertRepository : BaseRepository<AlertEntity>, IAlertRepository
{
    public AlertRepository(TrafficAnalyticsDbContext context)
        : base(context) { }

    public async Task<IEnumerable<AlertEntity>> GetRecentAlertsAsync(int limit = 20)
    {
        return await _context.Alerts
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AlertEntity>> GetByIntersectionAsync(int intersectionId)
    {
        return await _context.Alerts
            .Where(a => a.IntersectionId == intersectionId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteOldAlertsAsync(DateTime beforeDate)
    {
        var oldAlerts = await _context.Alerts
            .Where(a => a.CreatedAt < beforeDate)
            .ToListAsync();

        if (oldAlerts.Any())
        {
            _context.Alerts.RemoveRange(oldAlerts);
            await _context.SaveChangesAsync();
        }
    }
}