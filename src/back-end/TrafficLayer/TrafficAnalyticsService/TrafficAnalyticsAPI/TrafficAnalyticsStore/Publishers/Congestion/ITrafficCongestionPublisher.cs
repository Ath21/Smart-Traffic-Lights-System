using TrafficMessages;

namespace TrafficAnalyticsStore.Publishers.Congestion;

public interface ITrafficCongestionPublisher
{
    // traffic.analytics.congestion.{intersection_id}
    Task PublishCongestionAsync(TrafficCongestionMessage message);
}
