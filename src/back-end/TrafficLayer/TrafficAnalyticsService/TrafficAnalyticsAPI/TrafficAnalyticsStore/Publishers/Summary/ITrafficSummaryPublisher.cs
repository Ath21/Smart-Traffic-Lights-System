using TrafficMessages;

namespace TrafficAnalyticsStore.Publishers.Summary;

public interface ITrafficSummaryPublisher
{
    // traffic.analytics.summary.{intersection_id}
    Task PublishSummaryAsync(TrafficSummaryMessage message);
}