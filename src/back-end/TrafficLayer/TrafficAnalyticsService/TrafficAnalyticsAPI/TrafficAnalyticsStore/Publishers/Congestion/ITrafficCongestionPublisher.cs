using System;
using TrafficMessages;

namespace TrafficAnalyticsStore.Publishers.Congestion;

public interface ITrafficCongestionPublisher
{
    Task PublishCongestionAsync(TrafficCongestionMessage message);
}
