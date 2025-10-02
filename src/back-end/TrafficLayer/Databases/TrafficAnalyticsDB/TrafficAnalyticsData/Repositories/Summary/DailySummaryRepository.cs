using Microsoft.EntityFrameworkCore;
using TrafficAnalyticsData;
using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsData.Repositories.Summary;

public class DailySummaryRepository : Repository<DailySummary>, IDailySummaryRepository
{
    private readonly TrafficAnalyticsDbContext _context;

    public DailySummaryRepository(TrafficAnalyticsDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DailySummary>> GetByIntersectionAsync(int intersectionId) =>
        await _context.DailySummaries
            .Where(s => s.IntersectionId == intersectionId)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

    public async Task<DailySummary?> GetByIntersectionAndDateAsync(int intersectionId, DateTime date) =>
        await _context.DailySummaries
            .FirstOrDefaultAsync(s => s.IntersectionId == intersectionId && s.Date == date.Date);
}
