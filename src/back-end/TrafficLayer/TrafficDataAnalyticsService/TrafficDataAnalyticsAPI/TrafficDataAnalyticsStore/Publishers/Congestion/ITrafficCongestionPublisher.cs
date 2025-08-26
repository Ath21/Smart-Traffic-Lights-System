using System;
using TrafficMessages;

namespace TrafficDataAnalyticsStore.Publishers.Congestion;

public interface ITrafficCongestionPublisher
{
    Task PublishCongestionAsync(TrafficCongestionMessage message);
}
