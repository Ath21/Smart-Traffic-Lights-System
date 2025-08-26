using TrafficDataAnalyticsData.Entities;

namespace TrafficDataAnalyticsStore.Repository.Alerts;

public interface IAlertRepository
{
    Task<IEnumerable<Alert>> GetAllAsync();
    Task<Alert?> GetByIdAsync(Guid id);
    Task<IEnumerable<Alert>> GetByIntersectionAsync(Guid intersectionId);
    Task AddAsync(Alert alert);
    Task DeleteAsync(Guid id);
}
