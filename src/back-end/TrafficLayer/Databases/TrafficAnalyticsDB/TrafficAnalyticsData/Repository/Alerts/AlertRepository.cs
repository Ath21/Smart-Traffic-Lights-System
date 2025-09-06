using Microsoft.EntityFrameworkCore;
using TrafficAnalyticsData;
using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsStore.Repository.Alerts;

public class AlertRepository : IAlertRepository
{
    private readonly TrafficAnalyticsDbContext _context;

    public AlertRepository(TrafficAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Alert>> GetAllAsync() =>
        await _context.Alerts.AsNoTracking().ToListAsync();

    public async Task<Alert?> GetByIdAsync(Guid id) =>
        await _context.Alerts.AsNoTracking().FirstOrDefaultAsync(a => a.AlertId == id);

    public async Task<IEnumerable<Alert>> GetByIntersectionAsync(Guid intersectionId) =>
        await _context.Alerts.AsNoTracking().Where(a => a.IntersectionId == intersectionId).ToListAsync();

    public async Task AddAsync(Alert alert)
    {
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var alert = await _context.Alerts.FindAsync(id);
        if (alert != null)
        {
            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
        }
    }
}