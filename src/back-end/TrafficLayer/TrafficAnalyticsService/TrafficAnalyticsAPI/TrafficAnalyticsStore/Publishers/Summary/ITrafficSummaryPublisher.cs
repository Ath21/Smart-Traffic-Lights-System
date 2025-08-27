using System;
using TrafficMessages;

namespace TrafficAnalyticsStore.Publishers.Summary;

public interface ITrafficSummaryPublisher
{
    Task PublishSummaryAsync(TrafficSummaryMessage message);
}