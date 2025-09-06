using Microsoft.EntityFrameworkCore;
using TrafficAnalyticsData;
using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsStore.Repository.Summary;

public class DailySummaryRepository : IDailySummaryRepository
{
    private readonly TrafficAnalyticsDbContext _context;

    public DailySummaryRepository(TrafficAnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DailySummary>> GetAllAsync() =>
        await _context.DailySummaries.AsNoTracking().ToListAsync();

    public async Task<DailySummary?> GetByIdAsync(Guid id) =>
        await _context.DailySummaries.AsNoTracking().FirstOrDefaultAsync(d => d.SummaryId == id);

    public async Task<IEnumerable<DailySummary>> GetByIntersectionAsync(Guid intersectionId, DateTime? date = null)
    {
        var query = _context.DailySummaries.AsQueryable().Where(d => d.IntersectionId == intersectionId);

        if (date.HasValue)
            query = query.Where(d => d.Date.Date == date.Value.Date);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(DailySummary summary)
    {
        _context.DailySummaries.Add(summary);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DailySummary summary)
    {
        _context.DailySummaries.Update(summary);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var summary = await _context.DailySummaries.FindAsync(id);
        if (summary != null)
        {
            _context.DailySummaries.Remove(summary);
            await _context.SaveChangesAsync();
        }
    }
}
