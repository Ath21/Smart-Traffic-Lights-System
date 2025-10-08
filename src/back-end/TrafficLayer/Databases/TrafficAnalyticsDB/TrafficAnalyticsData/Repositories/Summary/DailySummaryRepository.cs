using Microsoft.EntityFrameworkCore;
using TrafficAnalyticsData;
using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsData.Repositories.Summary;

public class DailySummaryRepository : BaseRepository<DailySummaryEntity>, IDailySummaryRepository
{
    public DailySummaryRepository(TrafficAnalyticsDbContext context)
        : base(context) { }

    public async Task<IEnumerable<DailySummaryEntity>> GetByDateAsync(DateTime date)
    {
        return await _context.DailySummaries
            .Where(s => s.Date == date.Date)
            .OrderBy(s => s.Intersection)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailySummaryEntity>> GetByIntersectionAsync(int intersectionId)
    {
        return await _context.DailySummaries
            .Where(s => s.IntersectionId == intersectionId)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailySummaryEntity>> GetDateRangeAsync(DateTime start, DateTime end, int intersectionId)
    {
        return await _context.DailySummaries
            .Where(s => s.IntersectionId == intersectionId && s.Date >= start.Date && s.Date <= end.Date)
            .OrderBy(s => s.Date)
            .ToListAsync();
    }
}
