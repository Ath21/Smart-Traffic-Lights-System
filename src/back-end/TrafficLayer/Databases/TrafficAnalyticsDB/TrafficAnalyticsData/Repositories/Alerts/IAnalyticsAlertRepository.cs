using TrafficAnalyticsData.Entities;

namespace TrafficAnalyticsData.Repositories.Alerts;

public interface IAnalyticsAlertRepository : IRepository<Alert>
{
    Task<IEnumerable<Alert>> GetByIntersectionAsync(int intersectionId);
    Task<IEnumerable<Alert>> GetRecentAlertsAsync(int count = 10);
}

