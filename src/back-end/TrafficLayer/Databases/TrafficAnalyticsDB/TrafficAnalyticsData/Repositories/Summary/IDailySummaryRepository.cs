using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsData.Repositories.Summary;

public interface IDailySummaryRepository
{
    Task<IEnumerable<DailySummary>> GetAllAsync();
    Task<DailySummary?> GetByIdAsync(Guid id);
    Task<IEnumerable<DailySummary>> GetByIntersectionAsync(Guid intersectionId, DateTime? date = null);
    Task AddAsync(DailySummary summary);
    Task UpdateAsync(DailySummary summary);
    Task DeleteAsync(Guid id);
}
