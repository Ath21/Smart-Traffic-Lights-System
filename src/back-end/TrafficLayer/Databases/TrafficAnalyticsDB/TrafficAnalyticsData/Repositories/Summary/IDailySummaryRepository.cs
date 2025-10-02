using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsData.Repositories.Summary;


public interface IDailySummaryRepository : IRepository<DailySummary>
{
    Task<IEnumerable<DailySummary>> GetByIntersectionAsync(int intersectionId);
    Task<DailySummary?> GetByIntersectionAndDateAsync(int intersectionId, DateTime date);
}
